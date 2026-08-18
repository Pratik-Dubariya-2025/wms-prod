using WMS.Domain.Interfaces;

namespace WMS.Application.Common.Interfaces
{
    /// <summary>
    /// Maps PBAC module codes (e.g. "TASK_MGMT", "DEPT_MGMT") to the real domain entity
    /// they represent, so policy conditions/row-filters can be evaluated against actual
    /// database state instead of the raw incoming request DTO.
    /// </summary>
    public interface IPolicyResourceRegistry
    {
        /// <summary>True if the given module code has a registered entity mapping.</summary>
        bool IsModuleSupported(string moduleCode);

        /// <summary>
        /// Attempts to resolve a single-record entity ID out of a property bag (typically the
        /// resource dictionary already populated via reflection over the incoming request DTO)
        /// using the module's known ID property names (e.g. TASK_MGMT looks for "TaskId" then "Id").
        /// </summary>
        Guid? ResolveEntityId(string moduleCode, IReadOnlyDictionary<string, object> properties);

        /// <summary>
        /// Loads the real entity for the given module + ID via the shared IUnitOfWork repositories.
        /// Returns null if the module is unsupported, the ID doesn't resolve, or the entity is soft-deleted.
        /// </summary>
        Task<object?> LoadEntityAsync(string moduleCode, Guid entityId, IUnitOfWork unitOfWork);

        /// <summary>
        /// The CLR type backing a module, used for validating that policy condition/row-filter
        /// field names actually exist on the target entity before a policy is saved.
        /// </summary>
        Type? GetEntityType(string moduleCode);

        /// <summary>
        /// Validates if an attribute (either direct public property, standard primary key, or allowed virtual attribute)
        /// is supported for a given module code.
        /// </summary>
        bool IsAttributeSupported(string moduleCode, string attributeName);

        /// <summary>
        /// Retrieves a map of attribute aliases (e.g., Virtual Name -> Target Property Name) for a given module code.
        /// </summary>
        IReadOnlyDictionary<string, string> GetAttributeAliases(string moduleCode);

        /// <summary>
        /// Lets a module register a computed enrichment step that adds extra keys to the resource
        /// dictionary beyond a straight entity-property merge - e.g. USER_MGMT resolves the module
        /// a request's "PermissionCode" belongs to, exposed as "resource.PermissionModuleCode", so a
        /// policy can scope permission-management by module without a bespoke code path. No-op for
        /// modules that don't register one.
        /// </summary>
        Task EnrichContextAsync(string moduleCode, Dictionary<string, object> resource, IUnitOfWork unitOfWork);

        /// <summary>
        /// The full set of attribute names valid for "resource.*" bindings on this module: the
        /// entity's public scalar properties plus any registered virtual attributes. Backs the
        /// policy-editor attribute picker so authors choose from real fields instead of free text.
        /// </summary>
        IReadOnlyList<string> GetSupportedAttributes(string moduleCode);
    }
}
