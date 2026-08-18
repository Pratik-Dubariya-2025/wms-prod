namespace WMS.Application.Features.Tasks.DTOs
{
    /// <summary>
    /// Lightweight DTO used in paginated task lists.
    /// </summary>
    public class TaskListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }

        // Resolved names for display
        public string ProjectName { get; set; } = null!;
        public string TeamName { get; set; } = null!;
        public string? AssigneeName { get; set; }
        public Guid? AssigneeId { get; set; }
        public string CreatedByName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
