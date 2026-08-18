namespace WMS.Domain.Models
{
    public class Permission : BaseModel
    {
        public Guid ModuleId { get; set; }
        public Module Module { get; set; } = null!;

        public string Action { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }

        public List<RolePermission> RolePermissions { get; set; } = [];
    }
}
