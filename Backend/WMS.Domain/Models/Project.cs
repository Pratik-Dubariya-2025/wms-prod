namespace WMS.Domain.Models
{
    public class Project : BaseModel
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Planning"; // Planning, Active, OnHold, Completed, Cancelled
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // Manager who owns/created the project
        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        // One project has exactly one Team Lead
        public Guid? TeamLeadId { get; set; }
        public User? TeamLead { get; set; }

        // The team that owns this project — derived from the assigned Team Lead's team.
        // One project has exactly one team; one team lead has exactly one team.
        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }

        // One-to-Many: A project has many tasks
        public List<TaskItem> Tasks { get; set; } = [];

        // Many-to-Many via ProjectMember: users assigned to this project
        public List<ProjectMember> Members { get; set; } = [];
    }
}
