namespace WMS.Domain.Models
{
    public class Team : BaseModel
    {
        public string Name { get; set; } = null!;

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public Guid? TeamLeadId { get; set; }
        public User? TeamLead { get; set; }

        // One-to-Many: A team has many tasks
        public List<TaskItem> Tasks { get; set; } = [];
    }
}
