namespace WMS.Application.Common.Interfaces
{
    /// <summary>
    /// Manages the column-level restrictions for the current request context.
    /// Acts as an abstraction over HttpContext.Items to avoid referencing ASP.NET Core in WMS.Application.
    /// </summary>
    public interface IFieldRestrictionManager
    {
        /// <summary>
        /// Registers field restrictions for the current request.
        /// </summary>
        void SetRestrictions(List<FieldRestrictionResult> restrictions);

        /// <summary>
        /// Retrieves the registered field restrictions for the current request.
        /// </summary>
        List<FieldRestrictionResult> GetRestrictions();

        /// <summary>
        /// Gets or sets a value indicating whether the current request is authorized by PBAC.
        /// </summary>
        bool IsPbacAuthorized { get; set; }

        /// <summary>
        /// The PBAC module code the current request was evaluated against. Lets the response-side
        /// field restriction filter scope Hide/Mask matching to that module's entity, instead of
        /// matching any property with the same name anywhere in the response tree.
        /// </summary>
        string? ModuleCode { get; set; }
    }
}
