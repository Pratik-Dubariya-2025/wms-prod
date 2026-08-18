namespace WMS.Application.Features.Tasks.DTOs
{
    /// <summary>
    /// Full detail DTO for a single task view.
    /// </summary>
    public class TaskDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public decimal? EstimatedHours { get; set; }
        public DateTime? DueDate { get; set; }

        // Project info
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;

        // Team info
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }

        // Assignee info
        public Guid? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public string? AssigneeEmail { get; set; }

        // Creator info
        public Guid CreatedById { get; set; }
        public string CreatedByName { get; set; } = null!;

        // Audit
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
