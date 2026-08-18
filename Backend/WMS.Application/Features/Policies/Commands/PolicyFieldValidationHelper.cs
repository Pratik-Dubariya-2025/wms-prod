using System.Reflection;
using System.Text.Json;
using WMS.Application.Common.Interfaces;

namespace WMS.Application.Features.Policies.Commands
{
    /// <summary>
    /// Validates that policy condition attributes and row-filter fields reference real properties on
    /// the target module's entity (via <see cref="IPolicyResourceRegistry"/>), catching typos at save
    /// time instead of letting them silently produce an always-false (always-deny) filter later.
    /// </summary>
    public static class PolicyFieldValidationHelper
    {
        /// <summary>
        /// Mirrors PolicyEvaluationService's row-filter tree shape: either a clause (Field/Operator/
        /// Value) or a group (Logic/Children) whose children can themselves be clauses or groups.
        /// </summary>
        private class RowFilterNodeProbe
        {
            public string? Logic { get; set; }
            public List<RowFilterNodeProbe>? Children { get; set; }
            public string? Field { get; set; }
        }

        public static List<string> ValidateFieldNames(
            IPolicyResourceRegistry registry,
            string moduleCode,
            string? rowFilterExpression,
            IEnumerable<string> conditionAttributes)
        {
            var errors = new List<string>();

            var entityType = registry.GetEntityType(moduleCode);
            if (entityType == null)
            {
                // Module isn't in the resource registry yet - nothing to validate field names against.
                return errors;
            }

            bool HasProperty(string name) =>
                registry.IsAttributeSupported(moduleCode, name);

            foreach (var rawAttribute in conditionAttributes)
            {
                var attr = rawAttribute.Trim();
                if (attr.StartsWith('{') && attr.EndsWith('}'))
                {
                    attr = attr.Substring(1, attr.Length - 2).Trim();
                }

                if (attr.StartsWith("resource.", StringComparison.OrdinalIgnoreCase))
                {
                    var field = attr.Substring("resource.".Length);
                    if (!HasProperty(field))
                    {
                        errors.Add($"Condition attribute 'resource.{field}' does not exist on the target entity for module '{moduleCode}'.");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(rowFilterExpression))
            {
                try
                {
                    var root = JsonSerializer.Deserialize<RowFilterNodeProbe>(
                        rowFilterExpression, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (root != null)
                    {
                        ValidateRowFilterNode(root, moduleCode, HasProperty, errors);
                    }
                }
                catch (JsonException)
                {
                    errors.Add("RowFilterExpression is not valid JSON.");
                }
            }

            return errors;
        }

        private static void ValidateRowFilterNode(
            RowFilterNodeProbe node,
            string moduleCode,
            Func<string, bool> hasProperty,
            List<string> errors)
        {
            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    ValidateRowFilterNode(child, moduleCode, hasProperty, errors);
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(node.Field) && !hasProperty(node.Field))
            {
                errors.Add($"Row filter field '{node.Field}' does not exist on the target entity for module '{moduleCode}'.");
            }
        }
    }
}
