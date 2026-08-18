namespace WMS.Domain.Models
{
    public class Role : BaseModel
    {
        public string Name { get; set; } = null!;
        /// <summary>Short stable code, e.g. ADMIN, HR, TL. Unique.</summary>
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        /// <summary>Authority rank for hierarchy conflict resolution. Lower number = higher authority.</summary>
        public int Priority { get; set; } = 100;

        // Many-to-Many: A role can be assigned to multiple users
        public List<UserRole> UserRoles { get; set; } = [];
        public List<RolePermission> RolePermissions { get; set; } = [];
    }
}
