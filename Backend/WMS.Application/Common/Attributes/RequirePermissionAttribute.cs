namespace WMS.Application.Common.Attributes
{
    /// <summary>
    /// Marks a MediatR request as requiring one or more permissions.
    /// The <see cref="Behaviours.AuthorizationBehaviour{TRequest, TResponse}"/> 
    /// pipeline behavior reads this attribute and enforces the check 
    /// before the handler executes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RequirePermissionAttribute : Attribute
    {
        /// <summary>
        /// The permission codes required to execute this request.
        /// If multiple codes are specified, the user must have ALL of them.
        /// </summary>
        public string[] Permissions { get; }

        /// <summary>
        /// When true, the user needs at least one of the specified permissions (OR logic).
        /// When false (default), the user must have all specified permissions (AND logic).
        /// </summary>
        public bool RequireAny { get; set; } = false;

        public RequirePermissionAttribute(params string[] permissions)
        {
            Permissions = permissions;
        }
    }
}
