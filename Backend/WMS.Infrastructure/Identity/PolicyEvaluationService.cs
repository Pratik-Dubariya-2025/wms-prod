using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using WMS.Application.Common.Interfaces;
using WMS.Application.Features.Policies.DTOs;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Identity
{
    public class PolicyEvaluationService(
        IPolicyCacheService policyCache,
        IUnitOfWork unitOfWork,
        IPolicyResourceRegistry resourceRegistry,
        IUserHierarchyService userHierarchyService) : IPolicyEvaluationService
    {
        private readonly IPolicyCacheService _policyCache = policyCache;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPolicyResourceRegistry _resourceRegistry = resourceRegistry;
        private readonly IUserHierarchyService _userHierarchyService = userHierarchyService;

        /// <summary>
        /// Bundles every org-chart hierarchy set a policy might reference via a "user.*" dynamic
        /// binding, fetched once per evaluation call and threaded through instead of hitting the
        /// hierarchy service separately for each binding a policy happens to use.
        /// </summary>
        private sealed record HierarchyContext(IReadOnlySet<Guid> SubordinateIds, IReadOnlySet<Guid> ReportingTeamIds);

        private async Task<HierarchyContext> BuildHierarchyContextAsync(Guid userId)
        {
            var subordinatesTask = _userHierarchyService.GetSubordinateIdsAsync(userId);
            var reportingTeamTask = _userHierarchyService.GetReportingTeamIdsAsync(userId);
            await Task.WhenAll(subordinatesTask, reportingTeamTask);
            return new HierarchyContext(subordinatesTask.Result, reportingTeamTask.Result);
        }

        public async Task<PolicyEvaluationResult> EvaluateAsync(
            Guid userId,
            string moduleCode,
            string action,
            object? resourceContext = null)
        {
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return new PolicyEvaluationResult
                {
                    IsAllowed = false,
                    DenialReason = "User not found.",
                    NoPoliciesExist = false
                };
            }

            var allUserPolicies = await _policyCache.GetPoliciesForUserAsync(userId);
            var matchingPolicies = FilterByModuleAndAction(allUserPolicies, moduleCode, action);

            var hierarchy = await BuildHierarchyContextAsync(userId);

            return await EvaluateForUserAsync(user, moduleCode, action, matchingPolicies, resourceContext, hierarchy);
        }

        public async Task<List<PolicyEvaluationResult>> EvaluateBatchAsync(
            Guid userId,
            string moduleCode,
            string action,
            IEnumerable<object?> resourceContexts)
        {
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return resourceContexts
                    .Select(_ => new PolicyEvaluationResult { IsAllowed = false, DenialReason = "User not found.", NoPoliciesExist = false })
                    .ToList();
            }

            // Fetch the policy cache and hierarchy sets exactly once for the whole batch, instead
            // of once per resource.
            var allUserPolicies = await _policyCache.GetPoliciesForUserAsync(userId);
            var matchingPolicies = FilterByModuleAndAction(allUserPolicies, moduleCode, action);
            var hierarchy = await BuildHierarchyContextAsync(userId);

            var results = new List<PolicyEvaluationResult>();
            foreach (var resourceContext in resourceContexts)
            {
                results.Add(await EvaluateForUserAsync(user, moduleCode, action, matchingPolicies, resourceContext, hierarchy));
            }
            return results;
        }

        private static List<AccessPolicyDto> FilterByModuleAndAction(List<AccessPolicyDto> policies, string moduleCode, string action) =>
            policies
                .Where(p => string.Equals(p.ModuleCode, moduleCode, StringComparison.OrdinalIgnoreCase)
                          && string.Equals(p.Action, action, StringComparison.OrdinalIgnoreCase))
                .ToList();

        private async Task<PolicyEvaluationResult> EvaluateForUserAsync(
            User user,
            string moduleCode,
            string action,
            List<AccessPolicyDto> matchingPolicies,
            object? resourceContext,
            HierarchyContext hierarchy)
        {
            if (!matchingPolicies.Any())
            {
                return new PolicyEvaluationResult { NoPoliciesExist = true };
            }

            // Enrich resource context (load the real target entity for this module if an ID is present)
            var enrichedResource = await BuildResourceDictionaryAsync(moduleCode, resourceContext);

            // Evaluate conditions and row filter against the resource context
            var matchedPolicies = matchingPolicies
                .Where(p => EvaluateConditions(p.Conditions, user, enrichedResource, hierarchy) &&
                            EvaluateRowFilter(p.RowFilterExpression, user, enrichedResource, hierarchy))
                .ToList();

            // 1. Check for explicit deny
            var denyPolicies = matchedPolicies.Where(p => string.Equals(p.Effect, "Deny", StringComparison.OrdinalIgnoreCase)).ToList();
            if (denyPolicies.Any())
            {
                return new PolicyEvaluationResult
                {
                    IsAllowed = false,
                    DenialReason = $"Access denied by policy: {denyPolicies.First().Name}",
                    NoPoliciesExist = false
                };
            }

            // 2. Check for allow
            var allowPolicies = matchedPolicies.Where(p => string.Equals(p.Effect, "Allow", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!allowPolicies.Any())
            {
                return new PolicyEvaluationResult
                {
                    IsAllowed = false,
                    DenialReason = "No matching Allow policies found.",
                    NoPoliciesExist = false
                };
            }

            // 3. Aggregate field restrictions
            var restrictions = allowPolicies
                .SelectMany(p => p.FieldRestrictions)
                .GroupBy(f => f.FieldName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new FieldRestrictionResult
                {
                    FieldName = g.Key,
                    // If multiple restrictions exist for the same field, choose the most restrictive: Hide > Mask > ReadOnly
                    RestrictionType = GetMostRestrictiveType(g.Select(r => r.RestrictionType)),
                    MaskPattern = g.FirstOrDefault(r => !string.IsNullOrEmpty(r.MaskPattern))?.MaskPattern
                })
                .ToList();

            // 4. Enforce ReadOnly restrictions on writes: if the incoming Create/Update request is
            // trying to actually change a ReadOnly-restricted field (its submitted value differs from
            // what's currently stored), deny. Echoing back the unchanged value is harmless and allowed.
            var readOnlyDenial = CheckReadOnlyViolation(action, resourceContext, enrichedResource, restrictions);
            if (readOnlyDenial != null)
            {
                return readOnlyDenial;
            }

            return new PolicyEvaluationResult
            {
                IsAllowed = true,
                NoPoliciesExist = false,
                FieldRestrictions = restrictions
            };
        }

        private static PolicyEvaluationResult? CheckReadOnlyViolation(
            string action,
            object? resourceContext,
            Dictionary<string, object> enrichedResource,
            List<FieldRestrictionResult> restrictions)
        {
            bool isWrite = string.Equals(action, "Create", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(action, "Update", StringComparison.OrdinalIgnoreCase);
            if (!isWrite || resourceContext == null) return null;

            var readOnlyFields = restrictions
                .Where(r => string.Equals(r.RestrictionType, "ReadOnly", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.FieldName);

            foreach (var fieldName in readOnlyFields)
            {
                var requestProp = resourceContext.GetType().GetProperty(
                    fieldName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (requestProp == null) continue;

                var requestVal = requestProp.GetValue(resourceContext);
                if (requestVal == null) continue; // field not supplied on this request - nothing to enforce

                enrichedResource.TryGetValue(fieldName, out var currentVal);
                var requestStr = requestVal.ToString();
                var currentStr = currentVal?.ToString();

                if (!string.Equals(requestStr, currentStr, StringComparison.OrdinalIgnoreCase))
                {
                    return new PolicyEvaluationResult
                    {
                        IsAllowed = false,
                        DenialReason = $"Field '{fieldName}' is read-only under your current access policy.",
                        NoPoliciesExist = false
                    };
                }
            }

            return null;
        }

        private async Task<Dictionary<string, object>> BuildResourceDictionaryAsync(
            string moduleCode,
            object? resourceContext)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (resourceContext == null)
            {
                dict["$IsListRequest"] = false;
                return dict;
            }

            // 1. Copy properties from the raw request DTO as a fallback layer. Real entity properties
            // loaded in step 2 below take precedence over these when both exist for the same key.
            if (resourceContext is Dictionary<string, object> resourceDict)
            {
                foreach (var kvp in resourceDict)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }
            else if (resourceContext is JsonElement jsonEl)
            {
                if (jsonEl.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in jsonEl.EnumerateObject())
                    {
                        switch (prop.Value.ValueKind)
                        {
                            case JsonValueKind.String:
                                dict[prop.Name] = prop.Value.GetString() ?? "";
                                break;
                            case JsonValueKind.Number:
                                dict[prop.Name] = prop.Value.GetDouble();
                                break;
                            case JsonValueKind.True:
                                dict[prop.Name] = true;
                                break;
                            case JsonValueKind.False:
                                dict[prop.Name] = false;
                                break;
                            case JsonValueKind.Null:
                                dict[prop.Name] = null!;
                                break;
                            default:
                                dict[prop.Name] = prop.Value.ToString();
                                break;
                        }
                    }
                }
            }
            else
            {
                var props = resourceContext.GetType().GetProperties(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                foreach (var prop in props)
                {
                    try
                    {
                        var val = prop.GetValue(resourceContext);
                        if (val != null)
                        {
                            dict[prop.Name] = val;
                        }
                    }
                    catch
                    {
                        // Ignore properties that fail to evaluate
                    }
                }
            }

            // Determine if this represents a list/paginated request: a Query-suffixed type with no
            // resolvable single-record ID for this module is treated as a list request. The registry
            // (item: PolicyResourceRegistry) knows each module's ID property names, so this works
            // uniformly instead of hardcoding a handful of Id/RoleId/UserId property names.
            var typeName = resourceContext.GetType().Name;
            bool isList;
            if (typeName.Equals("GetRolePermissionsQuery", StringComparison.OrdinalIgnoreCase))
            {
                isList = true;
            }
            else if (typeName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
            {
                isList = _resourceRegistry.ResolveEntityId(moduleCode, dict) == null;
            }
            else
            {
                // Commands (Create/Update/Delete/Approve) always target a single record.
                isList = false;
            }
            dict["$IsListRequest"] = isList;

            // 2. Enrich with the real target entity for this module, if the registry supports it and
            // an ID can be resolved from the request. This is what makes conditions/row-filters like
            // "resource.AssigneeId" or "resource.DepartmentId" work for every module, not just Role/User.
            if (!isList && _resourceRegistry.IsModuleSupported(moduleCode))
            {
                var entityId = _resourceRegistry.ResolveEntityId(moduleCode, dict);
                if (entityId.HasValue)
                {
                    var entity = await _resourceRegistry.LoadEntityAsync(moduleCode, entityId.Value, _unitOfWork);
                    if (entity != null)
                    {
                        MergeEntityProperties(dict, entity);

                        // Backward-compatible prefixed convenience keys for ROLE_MGMT/USER_MGMT policies
                        // authored against the older field names.
                        if (entity is Role role)
                        {
                            dict["RoleCode"] = role.Code;
                            dict["RoleName"] = role.Name;
                            dict["RolePriority"] = role.Priority;
                            dict["RoleDescription"] = role.Description ?? "";
                        }
                        else if (entity is User targetUser)
                        {
                            dict["TargetUserId"] = targetUser.Id;
                            dict["TargetUsername"] = targetUser.Username;
                            dict["TargetEmail"] = targetUser.Email;
                            dict["TargetDepartmentId"] = targetUser.DepartmentId;
                            dict["TargetDesignationId"] = targetUser.DesignationId;
                            dict["TargetManagerId"] = targetUser.ManagerId ?? Guid.Empty;
                        }
                    }
                }

                // Module-specific computed attributes that need more than a straight entity-property
                // merge (e.g. USER_MGMT's "resource.PermissionModuleCode" - which module a PermissionCode
                // in the request belongs to). No-op for modules that don't register an enricher.
                await _resourceRegistry.EnrichContextAsync(moduleCode, dict, _unitOfWork);
            }

            // Apply aliases registered for this module
            if (_resourceRegistry.IsModuleSupported(moduleCode))
            {
                var aliases = _resourceRegistry.GetAttributeAliases(moduleCode);
                foreach (var aliasKvp in aliases)
                {
                    if (dict.TryGetValue(aliasKvp.Value, out var targetVal) && !dict.ContainsKey(aliasKvp.Key))
                    {
                        dict[aliasKvp.Key] = targetVal;
                    }
                    else if (dict.TryGetValue(aliasKvp.Key, out var aliasVal) && !dict.ContainsKey(aliasKvp.Value))
                    {
                        dict[aliasKvp.Value] = aliasVal;
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Merges an entity's public scalar properties into the resource dictionary, unprefixed,
        /// so "resource.&lt;EntityPropertyName&gt;" resolves against real database state.
        /// </summary>
        private static void MergeEntityProperties(Dictionary<string, object> dict, object entity)
        {
            var props = entity.GetType().GetProperties(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in props)
            {
                // Skip navigation properties (entities/collections) - only scalar values are relevant
                // for condition/row-filter comparisons.
                if (!IsScalarType(prop.PropertyType)) continue;

                try
                {
                    var val = prop.GetValue(entity);
                    if (val != null)
                    {
                        dict[prop.Name] = val;
                    }
                }
                catch
                {
                    // Ignore properties that fail to evaluate
                }
            }
        }

        private static bool IsScalarType(Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(Guid) ||
                   t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(decimal) ||
                   t == typeof(TimeSpan);
        }

        public async Task<Expression<Func<T, bool>>?> GetRowFilterExpressionAsync<T>(
            Guid userId,
            string moduleCode,
            string action) where T : class
        {
            var listFilter = await GetListFilterAsync<T>(userId, moduleCode, action);
            return listFilter.Filter;
        }

        public async Task<PbacListFilterResult<T>> GetListFilterAsync<T>(
            Guid userId,
            string moduleCode,
            string action) where T : class
        {
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return new PbacListFilterResult<T> { PbacGoverned = true, Filter = x => false };
            }

            var allUserPolicies = await _policyCache.GetPoliciesForUserAsync(userId);

            var matchingPolicies = allUserPolicies
                .Where(p => string.Equals(p.ModuleCode, moduleCode, StringComparison.OrdinalIgnoreCase)
                         && string.Equals(p.Action, action, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!matchingPolicies.Any())
            {
                // No PBAC policies at all for this module+action -> caller should fall back to legacy RBAC scoping.
                return new PbacListFilterResult<T> { PbacGoverned = false, Filter = null };
            }

            var hierarchy = await BuildHierarchyContextAsync(userId);
            var listContext = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { ["$IsListRequest"] = true };

            // Explicit Deny always wins, including for list access - mirrors EvaluateAsync's single-record semantics.
            var denyPolicies = matchingPolicies
                .Where(p => string.Equals(p.Effect, "Deny", StringComparison.OrdinalIgnoreCase)
                         && EvaluateConditions(p.Conditions, user, listContext, hierarchy))
                .ToList();
            if (denyPolicies.Any())
            {
                return new PbacListFilterResult<T> { PbacGoverned = true, Filter = x => false };
            }

            // Filter Allow policies whose conditions match (without specific resource context, but indicating a list request)
            var activeAllowPolicies = matchingPolicies
                .Where(p => string.Equals(p.Effect, "Allow", StringComparison.OrdinalIgnoreCase)
                         && EvaluateConditions(p.Conditions, user, listContext, hierarchy))
                .ToList();
            if (!activeAllowPolicies.Any())
            {
                return new PbacListFilterResult<T> { PbacGoverned = true, Filter = x => false };
            }

            var expressions = new List<Expression<Func<T, bool>>>();
            bool hasUnrestrictedAllow = false;

            foreach (var policy in activeAllowPolicies)
            {
                if (string.IsNullOrEmpty(policy.RowFilterExpression))
                {
                    // An Allow policy without any row filter means unrestricted access
                    hasUnrestrictedAllow = true;
                    break;
                }

                try
                {
                    var rootNode = JsonSerializer.Deserialize<RowFilterNodeJson>(policy.RowFilterExpression, JsonOptions);

                    if (rootNode != null)
                    {
                        var expr = BuildExpression<T>(rootNode, user, hierarchy);
                        if (expr != null)
                        {
                            expressions.Add(expr);
                        }
                    }
                }
                catch
                {
                    // Fail-closed on invalid JSON row filter format
                    expressions.Add(x => false);
                }
            }

            if (hasUnrestrictedAllow)
            {
                return new PbacListFilterResult<T> { PbacGoverned = true, Filter = null };
            }

            if (!expressions.Any())
            {
                return new PbacListFilterResult<T> { PbacGoverned = true, Filter = x => false };
            }

            // Combine multiple Allow row filters with OR
            return new PbacListFilterResult<T> { PbacGoverned = true, Filter = CombineOr(expressions) };
        }

        public async Task<PolicyPreviewResult> PreviewEvaluationAsync(
            Guid userId,
            string moduleCode,
            string action,
            string? resourceContextJson = null)
        {
            var result = new PolicyPreviewResult();

            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                result.IsAllowed = false;
                result.DenialReason = "User not found.";
                return result;
            }

            object? resourceObj = null;
            if (!string.IsNullOrWhiteSpace(resourceContextJson))
            {
                try
                {
                    resourceObj = JsonSerializer.Deserialize<JsonElement>(resourceContextJson);
                }
                catch (Exception ex)
                {
                    result.IsAllowed = false;
                    result.DenialReason = $"Invalid resource context JSON: {ex.Message}";
                    return result;
                }
            }

            var allUserPolicies = await _policyCache.GetPoliciesForUserAsync(userId);

            var matchingPolicies = allUserPolicies
                .Where(p => string.Equals(p.ModuleCode, moduleCode, StringComparison.OrdinalIgnoreCase)
                          && string.Equals(p.Action, action, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!matchingPolicies.Any())
            {
                result.NoPoliciesExist = true;
                result.IsAllowed = false;
                result.DenialReason = "No matching policies found for this module and action.";
                return result;
            }

            result.NoPoliciesExist = false;

            // Enrich resource context (load role/user details if ID is present)
            var enrichedResource = await BuildResourceDictionaryAsync(moduleCode, resourceObj);
            var hierarchy = await BuildHierarchyContextAsync(userId);

            // Evaluate policies step-by-step
            foreach (var policy in matchingPolicies)
            {
                var policyDetail = new EvaluatedPolicyDetail
                {
                    PolicyId = policy.Id,
                    PolicyName = policy.Name,
                    Effect = policy.Effect,
                    Priority = policy.Priority
                };

                bool passedAllGroups = true;
                if (policy.Conditions != null && policy.Conditions.Any())
                {
                    var groupConds = policy.Conditions.GroupBy(c => c.ConditionGroup);
                    var groupPassedList = new List<bool>();

                    foreach (var group in groupConds)
                    {
                        bool groupPassed = true;
                        foreach (var cond in group)
                        {
                            object? leftVal = ResolveValue(cond.Attribute, user, enrichedResource, hierarchy);
                            object? rightVal = ResolveValue(cond.Value, user, enrichedResource, hierarchy);
                            bool passed = CompareValues(leftVal, rightVal, cond.Operator);

                            policyDetail.Conditions.Add(new EvaluatedConditionDetail
                            {
                                ConditionGroup = cond.ConditionGroup,
                                Attribute = cond.Attribute,
                                Operator = cond.Operator,
                                Value = cond.Value,
                                ResolvedLeftValue = DescribeResolvedValue(leftVal),
                                ResolvedRightValue = DescribeResolvedValue(rightVal),
                                Passed = passed
                            });

                            if (!passed)
                            {
                                groupPassed = false;
                            }
                        }
                        groupPassedList.Add(groupPassed);
                    }

                    passedAllGroups = groupPassedList.Any(g => g);
                }

                // Also gate on the row filter, mirroring EvaluateAsync, so the preview accurately
                // reflects what a single-record access check would decide.
                if (passedAllGroups)
                {
                    passedAllGroups = EvaluateRowFilter(policy.RowFilterExpression, user, enrichedResource, hierarchy);
                }

                policyDetail.PassedConditions = passedAllGroups;
                result.EvaluatedPolicies.Add(policyDetail);
            }

            // Apply ordering (Priority ASC, then Effect)
            var activePolicies = result.EvaluatedPolicies
                .Where(p => p.PassedConditions)
                .OrderBy(p => p.Priority)
                .ToList();

            // 1. Explicit deny check
            var denyPolicy = activePolicies.FirstOrDefault(p => string.Equals(p.Effect, "Deny", StringComparison.OrdinalIgnoreCase));
            if (denyPolicy != null)
            {
                result.IsAllowed = false;
                result.DenialReason = $"Explicit Deny wins from policy: '{denyPolicy.PolicyName}'";
            }
            else
            {
                // 2. Allow check
                var allowPolicies = activePolicies.Where(p => string.Equals(p.Effect, "Allow", StringComparison.OrdinalIgnoreCase)).ToList();
                if (!allowPolicies.Any())
                {
                    result.IsAllowed = false;
                    result.DenialReason = "No matching Allow policies were met.";
                }
                else
                {
                    result.IsAllowed = true;

                    // Aggregate field restrictions from matched Allow policies
                    var allowedPolicyIds = allowPolicies.Select(p => p.PolicyId).ToList();
                    var matchedAllowPolicies = matchingPolicies.Where(p => allowedPolicyIds.Contains(p.Id)).ToList();

                    result.FinalFieldRestrictions = matchedAllowPolicies
                        .SelectMany(p => p.FieldRestrictions)
                        .GroupBy(f => f.FieldName, StringComparer.OrdinalIgnoreCase)
                        .Select(g => new FieldRestrictionResult
                        {
                            FieldName = g.Key,
                            RestrictionType = GetMostRestrictiveType(g.Select(r => r.RestrictionType)),
                            MaskPattern = g.FirstOrDefault(r => !string.IsNullOrEmpty(r.MaskPattern))?.MaskPattern
                        })
                        .ToList();

                    // Retrieve Row Filter description
                    var activeAllowFilters = matchedAllowPolicies
                        .Where(p => !string.IsNullOrEmpty(p.RowFilterExpression))
                        .Select(p => p.RowFilterExpression)
                        .ToList();

                    if (activeAllowFilters.Any())
                    {
                        result.ResolvedRowFilter = string.Join(" OR ", activeAllowFilters);
                    }
                    else
                    {
                        var anyUnrestricted = matchedAllowPolicies.Any(p => string.IsNullOrEmpty(p.RowFilterExpression));
                        if (anyUnrestricted)
                        {
                            result.ResolvedRowFilter = "Unrestricted (No row filter applied)";
                        }
                    }
                }
            }

            return result;
        }

        private static string? DescribeResolvedValue(object? value)
        {
            if (value == null) return null;
            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                var items = enumerable.Cast<object>().Select(o => o?.ToString() ?? "").ToList();
                return items.Count <= 5
                    ? string.Join(", ", items)
                    : string.Join(", ", items.Take(5)) + $", ... (+{items.Count - 5} more)";
            }
            return value.ToString();
        }

        /// <summary>
        /// Evaluates a policy's RowFilterExpression against a single-record resource. List requests
        /// skip this check here since row filtering for lists is instead pushed into the DB query via
        /// GetRowFilterExpressionAsync/GetListFilterAsync.
        /// </summary>
        private bool EvaluateRowFilter(string? rowFilterJsonStr, User user, Dictionary<string, object> resource, HierarchyContext hierarchy)
        {
            if (string.IsNullOrEmpty(rowFilterJsonStr)) return true;

            if (resource.TryGetValue("$IsListRequest", out var isListVal) && isListVal is bool isList && isList)
            {
                return true;
            }

            try
            {
                var rootNode = JsonSerializer.Deserialize<RowFilterNodeJson>(rowFilterJsonStr, JsonOptions);
                if (rootNode == null) return false;

                return EvaluateNode(rootNode, user, resource, hierarchy);
            }
            catch
            {
                return false; // Fail-closed on deserialization or evaluation error
            }
        }

        private bool EvaluateNode(RowFilterNodeJson node, User user, object? resource, HierarchyContext hierarchy)
        {
            if (node.IsGroup)
            {
                bool isOr = string.Equals(node.Logic, "OR", StringComparison.OrdinalIgnoreCase);
                if (node.Children!.Count == 0) return true; // empty group = no restriction

                return isOr
                    ? node.Children.Any(c => EvaluateNode(c, user, resource, hierarchy))
                    : node.Children.All(c => EvaluateNode(c, user, resource, hierarchy));
            }

            // Clause
            object? leftVal = ResolveValue($"resource.{node.Field}", user, resource, hierarchy);

            object? rightVal;
            if (string.Equals(node.ValueType, "dynamic", StringComparison.OrdinalIgnoreCase))
            {
                rightVal = ResolveValue(node.Value!, user, resource, hierarchy);
            }
            else
            {
                rightVal = node.Value;
            }

            return CompareValues(leftVal, rightVal, node.Operator!);
        }

        private bool EvaluateConditions(List<PolicyConditionDto> conditions, User user, object? resource, HierarchyContext hierarchy)
        {
            if (conditions == null || !conditions.Any()) return true;

            var groupResults = conditions
                .GroupBy(c => c.ConditionGroup)
                .Select(g => g.All(c => EvaluateCondition(c, user, resource, hierarchy)));

            return groupResults.Any(r => r);
        }
        private bool EvaluateCondition(PolicyConditionDto condition, User user, object? resource, HierarchyContext hierarchy)
        {
            string attr = condition.Attribute.Trim();
            if (attr.StartsWith('{') && attr.EndsWith('}'))
            {
                attr = attr.Substring(1, attr.Length - 2).Trim();
            }

            if (resource is Dictionary<string, object> dict &&
                dict.TryGetValue("$IsListRequest", out var isListVal) &&
                isListVal is bool isList && isList &&
                attr.StartsWith("resource.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            object? leftVal = ResolveValue(condition.Attribute, user, resource, hierarchy);
            object? rightVal = ResolveValue(condition.Value, user, resource, hierarchy);
            return CompareValues(leftVal, rightVal, condition.Operator);
        }

        /// <summary>
        /// Resolves a raw attribute/value string to its runtime value. Recognizes "user.X" (reflects
        /// off the current user, with the reserved "user.SubordinateIds" (ManagerId chain) and
        /// "user.ReportingTeamIds" (ReportingOfficerId chain) bindings resolving to the precomputed
        /// hierarchy sets instead of a property), "resource.X" (reflects off the enriched resource), a
        /// bare "{X}" (tries user then resource), or else treats it as a static literal.
        /// </summary>
        private object? ResolveValue(string rawValue, User user, object? resource, HierarchyContext hierarchy)
        {
            if (string.IsNullOrEmpty(rawValue)) return null;

            string key = rawValue.Trim();
            bool isBraced = key.StartsWith('{') && key.EndsWith('}');
            if (isBraced)
            {
                key = key.Substring(1, key.Length - 2).Trim();
            }

            if (key.StartsWith("user.", StringComparison.OrdinalIgnoreCase))
            {
                var propName = key.Substring(5);
                var hierarchySet = ResolveHierarchyBinding(propName, hierarchy);
                if (hierarchySet != null) return hierarchySet;
                return GetPropertyValue(user, propName);
            }

            if (key.StartsWith("resource.", StringComparison.OrdinalIgnoreCase))
            {
                var propName = key.Substring(9);
                return resource != null ? GetPropertyValue(resource, propName) : null;
            }

            if (isBraced)
            {
                var hierarchySet = ResolveHierarchyBinding(key, hierarchy);
                if (hierarchySet != null) return hierarchySet;
                var userVal = GetPropertyValue(user, key);
                if (userVal != null) return userVal;
                return resource != null ? GetPropertyValue(resource, key) : null;
            }

            return rawValue; // Static string
        }

        private static IReadOnlySet<Guid>? ResolveHierarchyBinding(string propName, HierarchyContext hierarchy)
        {
            if (string.Equals(propName, "SubordinateIds", StringComparison.OrdinalIgnoreCase)) return hierarchy.SubordinateIds;
            if (string.Equals(propName, "ReportingTeamIds", StringComparison.OrdinalIgnoreCase)) return hierarchy.ReportingTeamIds;
            return null;
        }

        private object? GetPropertyValue(object obj, string propName)
        {
            if (obj == null) return null;
            if (obj is JsonElement jsonEl)
            {
                if (jsonEl.ValueKind == JsonValueKind.Object && jsonEl.TryGetProperty(propName, out var val))
                {
                    switch (val.ValueKind)
                    {
                        case JsonValueKind.String: return val.GetString();
                        case JsonValueKind.Number: return val.GetDouble();
                        case JsonValueKind.True: return true;
                        case JsonValueKind.False: return false;
                        case JsonValueKind.Null: return null;
                        default: return val.ToString();
                    }
                }
                return null;
            }
            if (obj is Dictionary<string, object> dict)
            {
                if (dict.TryGetValue(propName, out var val)) return val;
                var key = dict.Keys.FirstOrDefault(k => string.Equals(k, propName, StringComparison.OrdinalIgnoreCase));
                if (key != null) return dict[key];
                return null;
            }
            var prop = obj.GetType().GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return prop?.GetValue(obj);
        }

        private bool CompareValues(object? left, object? right, string op)
        {
            op = op.ToLowerInvariant();

            // "in"/"not_in" accept a real collection on the right (e.g. {user.SubordinateIds}) as well
            // as the legacy comma-separated static string.
            if (op == "in" || op == "not_in")
            {
                var rightSet = ToStringSet(right);
                string leftStr0 = left?.ToString() ?? "";
                bool contained = left != null && rightSet.Any(v => string.Equals(leftStr0, v, StringComparison.OrdinalIgnoreCase));
                return op == "in" ? contained : !contained;
            }

            if (left == null && right == null) return op == "eq";
            if (left == null || right == null) return op == "neq";

            string leftStr = left.ToString() ?? "";
            string rightStr = right.ToString() ?? "";

            switch (op)
            {
                case "eq":
                    return string.Equals(leftStr, rightStr, StringComparison.OrdinalIgnoreCase);
                case "neq":
                    return !string.Equals(leftStr, rightStr, StringComparison.OrdinalIgnoreCase);
                case "contains":
                    return leftStr.Contains(rightStr, StringComparison.OrdinalIgnoreCase);
                case "gt":
                    if (double.TryParse(leftStr, out double lGt) && double.TryParse(rightStr, out double rGt))
                        return lGt > rGt;
                    return string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) > 0;
                case "lt":
                    if (double.TryParse(leftStr, out double lLt) && double.TryParse(rightStr, out double rLt))
                        return lLt < rLt;
                    return string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) < 0;
                case "gte":
                    if (double.TryParse(leftStr, out double lGte) && double.TryParse(rightStr, out double rGte))
                        return lGte >= rGte;
                    return string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) >= 0;
                case "lte":
                    if (double.TryParse(leftStr, out double lLte) && double.TryParse(rightStr, out double rLte))
                        return lLte <= rLte;
                    return string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) <= 0;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Flattens a resolved value into a set of comparable string tokens: a real collection (e.g.
        /// a hierarchy id set) is enumerated directly; a static value is split on commas, matching
        /// the legacy "a,b,c" literal-list convention.
        /// </summary>
        private static List<string> ToStringSet(object? value)
        {
            if (value == null) return [];
            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                return enumerable.Cast<object>()
                    .Select(o => o?.ToString() ?? "")
                    .Where(s => s.Length > 0)
                    .ToList();
            }
            return (value.ToString() ?? "")
                .Split(',')
                .Select(v => v.Trim())
                .Where(v => v.Length > 0)
                .ToList();
        }

        private string GetMostRestrictiveType(IEnumerable<string> types)
        {
            var set = new HashSet<string>(types, StringComparer.OrdinalIgnoreCase);
            if (set.Contains("Hide")) return "Hide";
            if (set.Contains("Mask")) return "Mask";
            if (set.Contains("ReadOnly")) return "ReadOnly";
            return "Hide";
        }

        // ---- Row filter tree -> SQL-translatable expression ----------------------------------

        private Expression<Func<T, bool>>? BuildExpression<T>(RowFilterNodeJson root, User user, HierarchyContext hierarchy)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var body = BuildNodeExpression<T>(root, parameter, user, hierarchy);
            if (body == null) return null;
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private Expression? BuildNodeExpression<T>(RowFilterNodeJson node, ParameterExpression parameter, User user, HierarchyContext hierarchy)
        {
            if (node.IsGroup)
            {
                bool isOr = string.Equals(node.Logic, "OR", StringComparison.OrdinalIgnoreCase);
                Expression? combined = null;

                foreach (var child in node.Children!)
                {
                    var childExpr = BuildNodeExpression<T>(child, parameter, user, hierarchy) ?? Expression.Constant(false);
                    combined = combined == null
                        ? childExpr
                        : (isOr ? Expression.OrElse(combined, childExpr) : Expression.AndAlso(combined, childExpr));
                }

                return combined ?? Expression.Constant(true); // empty group = no restriction
            }

            return BuildClauseExpression<T>(node, parameter, user, hierarchy);
        }

        private Expression BuildClauseExpression<T>(RowFilterNodeJson clause, ParameterExpression parameter, User user, HierarchyContext hierarchy)
        {
            var propertyInfo = typeof(T).GetProperty(clause.Field!, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (propertyInfo == null) return Expression.Constant(false);

            Expression left = Expression.Property(parameter, propertyInfo);
            var op = clause.Operator!.ToLowerInvariant();

            object? resolved = clause.Value;
            if (string.Equals(clause.ValueType, "dynamic", StringComparison.OrdinalIgnoreCase) ||
                (clause.Value != null && clause.Value.StartsWith('{') && clause.Value.EndsWith('}')))
            {
                resolved = ResolveValue(clause.Value!, user, null, hierarchy);
            }

            bool resolvedIsCollection = resolved is System.Collections.IEnumerable && resolved is not string;

            if (op == "in" || op == "not_in")
            {
                var tokens = ToStringSet(resolved);
                return BuildInExpression(left, propertyInfo.PropertyType, tokens, negate: op == "not_in");
            }

            // Every other operator expects a single scalar value - a collection here (e.g. someone
            // used {user.SubordinateIds} with "eq") is a policy-authoring mistake; fail closed.
            if (resolvedIsCollection) return Expression.Constant(false);

            if (resolved == null)
            {
                return op == "eq"
                    ? Expression.Equal(left, Expression.Constant(null, propertyInfo.PropertyType))
                    : Expression.Constant(false);
            }

            if (op == "contains" && propertyInfo.PropertyType == typeof(string))
            {
                var method = typeof(string).GetMethod("Contains", [typeof(string)])!;
                return Expression.Call(left, method, Expression.Constant(resolved.ToString(), typeof(string)));
            }

            object? typedValue;
            try
            {
                var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                typedValue = targetType == typeof(Guid) ? Guid.Parse(resolved.ToString()!) : Convert.ChangeType(resolved, targetType);
            }
            catch
            {
                return Expression.Constant(false);
            }

            Expression right = Expression.Constant(typedValue, propertyInfo.PropertyType);

            return op switch
            {
                "eq" => Expression.Equal(left, right),
                "neq" => Expression.NotEqual(left, right),
                "gt" => Expression.GreaterThan(left, right),
                "lt" => Expression.LessThan(left, right),
                "gte" => Expression.GreaterThanOrEqual(left, right),
                "lte" => Expression.LessThanOrEqual(left, right),
                "contains" => Expression.Equal(left, right), // non-string "contains" falls back to equality
                _ => Expression.Equal(left, right),
            };
        }

        /// <summary>
        /// Builds "typedList.Contains(x.Field)" - translates to a single SQL IN(...) instead of an
        /// OR-chain of equality checks, so this scales to large sets (e.g. a big subordinate tree).
        /// </summary>
        private static Expression BuildInExpression(Expression left, Type propertyType, List<string> rawValues, bool negate)
        {
            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var listType = typeof(List<>).MakeGenericType(propertyType);
            var list = (System.Collections.IList)Activator.CreateInstance(listType)!;

            foreach (var val in rawValues)
            {
                try
                {
                    object typed = targetType == typeof(Guid) ? Guid.Parse(val) : Convert.ChangeType(val, targetType);
                    list.Add(typed);
                }
                catch
                {
                    // Skip values that don't convert to the field's type
                }
            }

            if (list.Count == 0)
            {
                // "in nothing" never matches; "not_in nothing" always matches.
                return Expression.Constant(negate);
            }

            var containsMethod = listType.GetMethod("Contains", [propertyType])!;
            Expression comparison = Expression.Call(Expression.Constant(list, listType), containsMethod, left);
            return negate ? Expression.Not(comparison) : comparison;
        }

        private static Expression<Func<T, bool>> CombineOr<T>(List<Expression<Func<T, bool>>> expressions)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combined = null;

            foreach (var expr in expressions)
            {
                var visitor = new ParameterReplacer(expr.Parameters[0], parameter);
                var body = visitor.Visit(expr.Body);

                combined = combined == null ? body : Expression.OrElse(combined, body);
            }

            return Expression.Lambda<Func<T, bool>>(combined!, parameter);
        }

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// A row filter is either a single clause ({Field,Operator,Value,ValueType} - the legacy
        /// single-clause shape, still valid) or a group ({Logic,Children:[...]} where children can
        /// themselves be clauses or nested groups), enabling arbitrary AND/OR nesting.
        /// </summary>
        private sealed class RowFilterNodeJson
        {
            public string? Logic { get; set; }
            public List<RowFilterNodeJson>? Children { get; set; }

            public string? Field { get; set; }
            public string? Operator { get; set; }
            public string? Value { get; set; }
            public string? ValueType { get; set; }

            [JsonIgnore]
            public bool IsGroup => Children != null && Children.Count > 0;
        }

        private class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam = oldParam;
            private readonly ParameterExpression _newParam = newParam;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParam ? _newParam : base.VisitParameter(node);
            }
        }
    }
}
