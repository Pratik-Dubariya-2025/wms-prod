namespace WMS.Domain.Models
{
    public class Module : BaseModel
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public List<Permission> Permissions { get; set; } = [];
    }
}
