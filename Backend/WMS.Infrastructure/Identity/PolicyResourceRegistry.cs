using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Identity
{
    /// <summary>
    /// Central registry of PBAC module code -> domain entity mappings. Adding a new module to
    /// PBAC only requires a new entry here; no changes to PolicyEvaluationService are needed.
    /// </summary>
    public class PolicyResourceRegistry : IPolicyResourceRegistry
    {
        private sealed record ModuleResourceDefinition(
            Type EntityType,
            string[] IdPropertyNames,
            string[] VirtualAttributes,
            Dictionary<string, string> AttributeAliases,
            Func<IUnitOfWork, Guid, Task<object?>> Loader,
            Func<IUnitOfWork, Dictionary<string, object>, Task>? ContextEnricher = null);

        /// <summary>
        /// USER_MGMT enrichment: when a request carries a "PermissionCode" (e.g. granting/revoking a
        /// UserPermissionOverride), resolve which module that permission belongs to and expose it as
        /// "resource.PermissionModuleCode". This is what lets a delegated-admin policy scope permission
        /// management by module (e.g. "only TASK_MGMT/PROJECT_MGMT/LEAVE_MGMT") without a bespoke handler.
        /// </summary>
        private static async Task EnrichUserMgmtContextAsync(IUnitOfWork uow, Dictionary<string, object> resource)
        {
            if (!resource.TryGetValue("PermissionCode", out var codeObj) || codeObj is not string code || string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            var permission = await uow.Permission.GetFirstOrDefaultAsync(p => p.Code == code && !p.IsDeleted);
            if (permission == null) return;

            var module = await uow.Module.GetFirstOrDefaultAsync(m => m.Id == permission.ModuleId);
            if (module != null)
            {
                resource["PermissionModuleCode"] = module.Code;
            }
        }

        private static readonly Dictionary<string, ModuleResourceDefinition> Definitions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["USER_MGMT"] = new(
                    typeof(User),
                    ["UserId", "TargetUserId", "Id"],
                    ["Permission", "PermissionCode", "PermissionModuleCode"],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Permission"] = "PermissionCode" },
                    async (uow, id) => await uow.User.GetFirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted),
                    ContextEnricher: EnrichUserMgmtContextAsync),

                ["ROLE_MGMT"] = new(
                    typeof(Role),
                    ["RoleId", "Id", "UserId"],
                    ["Permission", "PermissionCode"],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Permission"] = "PermissionCode" },
                    async (uow, id) =>
                    {
                        var role = await uow.Role.GetFirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
                        if (role != null) return role;

                        var userRole = await uow.UserRole.IncludeAndGetFirstOrDefaultAsync<object>(
                            ur => ur.UserId == id && !ur.Role.IsDeleted,
                            ur => ur.Role
                        );
                        return userRole?.Role;
                    }),

                ["DEPT_MGMT"] = new(
                    typeof(Department),
                    ["DepartmentId", "Id"],
                    [],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    async (uow, id) => await uow.Department.GetFirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted)),

                ["TASK_MGMT"] = new(
                    typeof(TaskItem),
                    ["TaskId", "Id"],
                    [],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    async (uow, id) => await uow.TaskItem.GetFirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted)),

                ["PROJECT_MGMT"] = new(
                    typeof(Project),
                    ["ProjectId", "Id"],
                    [],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    async (uow, id) => await uow.Project.GetFirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)),

                ["HR_MGMT"] = new(
                    typeof(EmployeeProfile),
                    ["UserId", "EmployeeProfileId", "Id"],
                    [],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    async (uow, id) => await uow.EmployeeProfile.GetFirstOrDefaultAsync(e => (e.UserId == id || e.Id == id) && !e.IsDeleted)),

                ["LEAVE_MGMT"] = new(
                    typeof(LeaveRequest),
                    ["LeaveRequestId", "Id"],
                    [],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    async (uow, id) => await uow.LeaveRequest.GetFirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted)),

                ["CRM"] = new(
                    typeof(Lead),
                    ["LeadId", "Id"],
                    [],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    async (uow, id) => await uow.Lead.GetFirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted)),

                ["ACCOUNTS"] = new(
                    typeof(Invoice),
                    ["InvoiceId", "Id"],
                    [],
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    async (uow, id) => await uow.Invoice.GetFirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted)),
            };

        public bool IsModuleSupported(string moduleCode) => Definitions.ContainsKey(moduleCode);

        public Type? GetEntityType(string moduleCode) =>
            Definitions.TryGetValue(moduleCode, out var def) ? def.EntityType : null;

        public Guid? ResolveEntityId(string moduleCode, IReadOnlyDictionary<string, object> properties)
        {
            if (!Definitions.TryGetValue(moduleCode, out var def)) return null;

            foreach (var propName in def.IdPropertyNames)
            {
                var key = properties.Keys.FirstOrDefault(k => string.Equals(k, propName, StringComparison.OrdinalIgnoreCase));
                if (key == null) continue;

                var value = properties[key];
                if (value is Guid g && g != Guid.Empty) return g;
                if (value != null && Guid.TryParse(value.ToString(), out var parsed) && parsed != Guid.Empty) return parsed;
            }

            return null;
        }

        public async Task<object?> LoadEntityAsync(string moduleCode, Guid entityId, IUnitOfWork unitOfWork)
        {
            if (!Definitions.TryGetValue(moduleCode, out var def)) return null;
            return await def.Loader(unitOfWork, entityId);
        }

        public bool IsAttributeSupported(string moduleCode, string attributeName)
        {
            if (!Definitions.TryGetValue(moduleCode, out var def)) return false;

            // 1. Direct public properties on the entity type
            if (def.EntityType.GetProperty(attributeName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) != null)
                return true;

            // 2. Standard entity ID names
            if (attributeName.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                attributeName.Equals(def.EntityType.Name + "Id", StringComparison.OrdinalIgnoreCase))
                return true;

            // 3. Registered ID properties (e.g. TargetUserId)
            if (def.IdPropertyNames.Contains(attributeName, StringComparer.OrdinalIgnoreCase))
                return true;

            // 4. Registered virtual/request attributes
            if (def.VirtualAttributes.Contains(attributeName, StringComparer.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static readonly IReadOnlyDictionary<string, string> EmptyAliases = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> GetAttributeAliases(string moduleCode) =>
            Definitions.TryGetValue(moduleCode, out var def) ? def.AttributeAliases : EmptyAliases;

        public async Task EnrichContextAsync(string moduleCode, Dictionary<string, object> resource, IUnitOfWork unitOfWork)
        {
            if (!Definitions.TryGetValue(moduleCode, out var def) || def.ContextEnricher == null) return;
            await def.ContextEnricher(unitOfWork, resource);
        }

        private static readonly IReadOnlyList<string> EmptyAttributes = [];

        public IReadOnlyList<string> GetSupportedAttributes(string moduleCode)
        {
            if (!Definitions.TryGetValue(moduleCode, out var def)) return EmptyAttributes;

            var scalarProps = def.EntityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => IsScalarType(p.PropertyType))
                .Select(p => p.Name);

            return scalarProps
                .Concat(def.IdPropertyNames)
                .Concat(def.VirtualAttributes)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool IsScalarType(Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(Guid) ||
                   t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(decimal) ||
                   t == typeof(TimeSpan);
        }
    }
}
