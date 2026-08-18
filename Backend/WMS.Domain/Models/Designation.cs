namespace WMS.Domain.Models
{
    public class Designation : BaseModel
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public int Level { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public List<User> Users { get; set; } = [];
    }
}
