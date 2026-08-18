using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WMS.Application.Common.Interfaces;

namespace WMS.API.Filters
{
    /// <summary>
    /// ASP.NET Core Result Filter that applies column-level restrictions (masking and hiding fields)
    /// on response DTOs prior to serialization.
    /// </summary>
    public class FieldRestrictionFilter(
        IFieldRestrictionManager fieldRestrictionManager,
        IPolicyResourceRegistry resourceRegistry) : IAsyncResultFilter
    {
        private readonly IFieldRestrictionManager _fieldRestrictionManager = fieldRestrictionManager;
        private readonly IPolicyResourceRegistry _resourceRegistry = resourceRegistry;

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var restrictions = _fieldRestrictionManager.GetRestrictions();

            if (restrictions != null && restrictions.Any())
            {
                if (context.Result is ObjectResult objectResult && objectResult.Value != null)
                {
                    // Scope matching to the module's registered entity where possible, so a restriction
                    // named e.g. "Salary" only masks that field on the entity it was defined for, not
                    // any same-named property that happens to appear elsewhere in the response tree.
                    var moduleCode = _fieldRestrictionManager.ModuleCode;
                    var entityType = moduleCode != null ? _resourceRegistry.GetEntityType(moduleCode) : null;
                    ApplyRestrictions(objectResult.Value, restrictions, entityType);
                }
            }

            await next();
        }

        private void ApplyRestrictions(object? obj, List<FieldRestrictionResult> restrictions, Type? entityType)
        {
            if (obj == null) return;

            // 1. Handle arrays, lists, and collections
            if (obj is System.Collections.IEnumerable enumerable && obj is not string)
            {
                foreach (var item in enumerable)
                {
                    ApplyRestrictions(item, restrictions, entityType);
                }
                return;
            }

            var type = obj.GetType();

            // Prevent infinite recursion on primitives and System namespace types
            if (type.IsPrimitive
                || type == typeof(string)
                || type == typeof(Guid)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type.Namespace?.StartsWith("System") == true)
            {
                return;
            }

            // 2. Iterate properties of the current object
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in properties)
            {
                // Check if this specific property has a restriction. When the module's entity type is
                // known, only apply it to properties that actually exist on that entity - if it's
                // unknown (unregistered module), fall back to matching by name everywhere, which is
                // the safer failure mode for a mechanism whose whole purpose is hiding sensitive data.
                bool belongsToEntity = entityType == null ||
                    entityType.GetProperty(prop.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) != null;

                var restriction = belongsToEntity
                    ? restrictions.FirstOrDefault(r => string.Equals(r.FieldName, prop.Name, StringComparison.OrdinalIgnoreCase))
                    : null;

                if (restriction != null)
                {
                    if (string.Equals(restriction.RestrictionType, "Hide", StringComparison.OrdinalIgnoreCase))
                    {
                        if (prop.CanWrite)
                        {
                            var defaultValue = prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;
                            prop.SetValue(obj, defaultValue);
                        }
                    }
                    else if (string.Equals(restriction.RestrictionType, "Mask", StringComparison.OrdinalIgnoreCase))
                    {
                        if (prop.PropertyType == typeof(string) && prop.CanWrite)
                        {
                            var currentVal = prop.GetValue(obj) as string;
                            if (!string.IsNullOrEmpty(currentVal))
                            {
                                var maskedVal = MaskString(currentVal, restriction.MaskPattern);
                                prop.SetValue(obj, maskedVal);
                            }
                        }
                    }
                }
                else
                {
                    // Recursively apply to child objects or lists if no restriction is placed directly on this property
                    if (prop.CanRead)
                    {
                        try
                        {
                            var nestedVal = prop.GetValue(obj);
                            ApplyRestrictions(nestedVal, restrictions, entityType);
                        }
                        catch
                        {
                            // Avoid throwing reflection exceptions on property reads
                        }
                    }
                }
            }
        }

        private string MaskString(string value, string? pattern)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (!string.IsNullOrEmpty(pattern))
            {
                if (pattern.Equals("Last4", StringComparison.OrdinalIgnoreCase))
                {
                    if (value.Length <= 4) return value;
                    return new string('*', value.Length - 4) + value.Substring(value.Length - 4);
                }
                if (pattern.Equals("First4", StringComparison.OrdinalIgnoreCase))
                {
                    if (value.Length <= 4) return value;
                    return value.Substring(0, 4) + new string('*', value.Length - 4);
                }
            }

            // Default: Mask everything except the last 4 characters
            if (value.Length <= 4)
            {
                return new string('*', value.Length);
            }
            return new string('*', value.Length - 4) + value.Substring(value.Length - 4);
        }
    }
}
