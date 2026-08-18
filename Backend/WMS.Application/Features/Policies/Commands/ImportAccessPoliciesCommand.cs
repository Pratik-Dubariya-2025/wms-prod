using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Policies.Queries;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Policies.Commands
{
    [RequirePermission(PermissionCodes.PolicyWrite)]
    public class ImportAccessPoliciesCommand : IRequest<ApiResponse<ImportReportDto>>
    {
        public List<ExportPolicyDto> Policies { get; set; } = [];
        public bool OverwriteExisting { get; set; } = true;
    }

    public class ImportReportDto
    {
        public int TotalProcessed { get; set; }
        public int ImportedCount { get; set; }
        public int OverwrittenCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ImportAccessPoliciesCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyCacheService policyCache,
        IRealtimeNotificationService realtimeNotificationService)
        : IRequestHandler<ImportAccessPoliciesCommand, ApiResponse<ImportReportDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyCacheService _policyCache = policyCache;
        private readonly IRealtimeNotificationService _realtimeNotificationService = realtimeNotificationService;

        public async Task<ApiResponse<ImportReportDto>> Handle(ImportAccessPoliciesCommand request, CancellationToken cancellationToken)
        {
            var report = new ImportReportDto
            {
                TotalProcessed = request.Policies.Count
            };

            var affectedUserIds = new HashSet<Guid>();
            var affectedRoleIds = new HashSet<Guid>();

            foreach (var importDto in request.Policies)
            {
                try
                {
                    // 1. Resolve Module
                    var module = await _unitOfWork.Module.GetFirstOrDefaultAsync(m => m.Code.ToLower() == importDto.ModuleCode.ToLower() && !m.IsDeleted);
                    if (module == null)
                    {
                        report.SkippedCount++;
                        report.Errors.Add($"Skipped '{importDto.Name}': Module with code '{importDto.ModuleCode}' not found.");
                        continue;
                    }

                    // 2. Resolve target Role or User
                    Guid? roleId = null;
                    Guid? userId = null;

                    if (!string.IsNullOrEmpty(importDto.RoleName))
                    {
                        var role = await _unitOfWork.Role.GetFirstOrDefaultAsync(r => r.Name.ToLower() == importDto.RoleName.ToLower() && !r.IsDeleted);
                        if (role == null)
                        {
                            report.SkippedCount++;
                            report.Errors.Add($"Skipped '{importDto.Name}': Role '{importDto.RoleName}' not found.");
                            continue;
                        }
                        roleId = role.Id;
                    }
                    else if (!string.IsNullOrEmpty(importDto.UserEmail))
                    {
                        var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Email.ToLower() == importDto.UserEmail.ToLower() && !u.IsDeleted);
                        if (user == null)
                        {
                            report.SkippedCount++;
                            report.Errors.Add($"Skipped '{importDto.Name}': User with email '{importDto.UserEmail}' not found.");
                            continue;
                        }
                        userId = user.Id;
                    }
                    else
                    {
                        report.SkippedCount++;
                        report.Errors.Add($"Skipped '{importDto.Name}': Policy must target either a Role or a User.");
                        continue;
                    }

                    // 3. Check for existing policy by Name
                    var existingPolicy = await _unitOfWork.AccessPolicy.IncludeAndGetFirstOrDefaultAsync<object>(
                        p => p.Name.ToLower() == importDto.Name.ToLower() && !p.IsDeleted,
                        p => p.Conditions,
                        p => p.FieldRestrictions
                    );

                    if (existingPolicy != null)
                    {
                        if (!request.OverwriteExisting)
                        {
                            report.SkippedCount++;
                            report.Errors.Add($"Skipped '{importDto.Name}': Policy already exists and OverwriteExisting is false.");
                            continue;
                        }

                        // Overwrite existing
                        existingPolicy.Description = importDto.Description;
                        existingPolicy.ModuleId = module.Id;
                        existingPolicy.Action = importDto.Action;
                        existingPolicy.Effect = importDto.Effect;
                        existingPolicy.RoleId = roleId;
                        existingPolicy.UserId = userId;
                        existingPolicy.RowFilterExpression = importDto.RowFilterExpression;
                        existingPolicy.Priority = Math.Clamp(importDto.Priority, 1, 1000);
                        existingPolicy.IsActive = importDto.IsActive;
                        existingPolicy.ModifiedBy = _currentUserService.Username ?? "System";

                        // Clear old conditions & restrictions
                        foreach (var cond in existingPolicy.Conditions) _unitOfWork.PolicyCondition.Remove(cond);
                        foreach (var rest in existingPolicy.FieldRestrictions) _unitOfWork.FieldRestriction.Remove(rest);

                        // Add new conditions
                        foreach (var condDto in importDto.Conditions)
                        {
                            existingPolicy.Conditions.Add(new PolicyCondition
                            {
                                ConditionGroup = condDto.ConditionGroup,
                                Attribute = condDto.Attribute.Trim(),
                                Operator = condDto.Operator.Trim().ToLower(),
                                Value = condDto.Value.Trim(),
                                CreatedBy = _currentUserService.Username ?? "System"
                            });
                        }

                        // Add new restrictions
                        foreach (var restDto in importDto.FieldRestrictions)
                        {
                            existingPolicy.FieldRestrictions.Add(new FieldRestriction
                            {
                                FieldName = restDto.FieldName.Trim(),
                                RestrictionType = restDto.RestrictionType,
                                MaskPattern = restDto.MaskPattern,
                                CreatedBy = _currentUserService.Username ?? "System"
                            });
                        }

                        _unitOfWork.AccessPolicy.Update(existingPolicy);
                        report.OverwrittenCount++;
                    }
                    else
                    {
                        // Create new policy
                        var newPolicy = new AccessPolicy
                        {
                            Name = importDto.Name.Trim(),
                            Description = importDto.Description,
                            ModuleId = module.Id,
                            Action = importDto.Action.Trim(),
                            Effect = importDto.Effect,
                            RoleId = roleId,
                            UserId = userId,
                            RowFilterExpression = importDto.RowFilterExpression,
                            Priority = Math.Clamp(importDto.Priority, 1, 1000),
                            IsActive = importDto.IsActive,
                            CreatedBy = _currentUserService.Username ?? "System"
                        };

                        foreach (var condDto in importDto.Conditions)
                        {
                            newPolicy.Conditions.Add(new PolicyCondition
                            {
                                ConditionGroup = condDto.ConditionGroup,
                                Attribute = condDto.Attribute.Trim(),
                                Operator = condDto.Operator.Trim().ToLower(),
                                Value = condDto.Value.Trim(),
                                CreatedBy = _currentUserService.Username ?? "System"
                            });
                        }

                        foreach (var restDto in importDto.FieldRestrictions)
                        {
                            newPolicy.FieldRestrictions.Add(new FieldRestriction
                            {
                                FieldName = restDto.FieldName.Trim(),
                                RestrictionType = restDto.RestrictionType,
                                MaskPattern = restDto.MaskPattern,
                                CreatedBy = _currentUserService.Username ?? "System"
                            });
                        }

                        await _unitOfWork.AccessPolicy.AddAsync(newPolicy);
                        report.ImportedCount++;
                    }

                    if (userId.HasValue) affectedUserIds.Add(userId.Value);
                    if (roleId.HasValue) affectedRoleIds.Add(roleId.Value);
                }
                catch (Exception ex)
                {
                    report.SkippedCount++;
                    report.Errors.Add($"Error processing '{importDto.Name}': {ex.Message}");
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache and notify for affected roles & users
            foreach (var userId in affectedUserIds)
            {
                await _policyCache.InvalidateUserPoliciesAsync(userId);
                await _realtimeNotificationService.NotifyUserPermissionsChangedAsync(userId, cancellationToken);
            }
            foreach (var roleId in affectedRoleIds)
            {
                await _policyCache.InvalidateRolePoliciesAsync(roleId);
                await _realtimeNotificationService.NotifyRolePermissionsChangedAsync(roleId, cancellationToken);
            }

            return ApiResponse<ImportReportDto>.Success(report, "Policies import completed.");
        }
    }
}
