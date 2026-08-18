namespace WMS.Domain.Models
{
    /// <summary>
    /// Named "TaskItem" to avoid collision with System.Threading.Tasks.Task.
    /// Maps to the SRS "tasks" table (Section 4.4.2).
    /// </summary>
    public class TaskItem : BaseModel
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, InProgress, InReview, Done, Closed
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        public decimal? EstimatedHours { get; set; }
        public DateTime? DueDate { get; set; }

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }

        public Guid? AssigneeId { get; set; }
        public User? Assignee { get; set; }

        public Guid CreatedById { get; set; }
        public User CreatedByUser { get; set; } = null!;

        // One-to-Many: A task has many time logs
        public List<TimeLog> TimeLogs { get; set; } = [];
    }
}
