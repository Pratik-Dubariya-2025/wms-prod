namespace WMS.Domain.Models
{
    /// <summary>
    /// Column-level security: defines which fields are hidden, masked, or read-only for a given policy.
    /// </summary>
    public class FieldRestriction : BaseModel
    {
        public Guid PolicyId { get; set; }
        public AccessPolicy Policy { get; set; } = null!;

        /// <summary>
        /// The entity field name to restrict.
        /// Examples: "salary", "bankAccountNo", "panNumber", "emergencyContactPhone"
        /// </summary>
        public string FieldName { get; set; } = null!;

        /// <summary>
        /// Type of restriction: Hide (remove from response), Mask (partial display), ReadOnly (prevent writes).
        /// </summary>
        public string RestrictionType { get; set; } = "Hide";

        /// <summary>
        /// Mask pattern for partial display, e.g., "****1234".
        /// Only applicable when RestrictionType is "Mask".
        /// </summary>
        public string? MaskPattern { get; set; }
    }
}
