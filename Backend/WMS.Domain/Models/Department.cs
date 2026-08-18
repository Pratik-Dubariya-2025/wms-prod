namespace WMS.Domain.Models
{
    public class Department : BaseModel
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public List<User> Users { get; set; } = [];
        public List<Designation> Designations { get; set; } = [];
    }
}
