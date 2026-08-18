namespace WMS.Application.Features.Projects.DTOs
{
    /// <summary>
    /// Full project detail DTO with task status summary.
    /// </summary>
    public class ProjectDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;

        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = null!;

        public Guid? TeamLeadId { get; set; }
        public string? TeamLeadName { get; set; }
        public Guid? TeamId { get; set; }

        public int MemberCount { get; set; }
        public TaskStatusSummary TaskSummary { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class TaskStatusSummary
    {
        public int Total { get; set; }
        public int Draft { get; set; }
        public int InProgress { get; set; }
        public int InReview { get; set; }
        public int Done { get; set; }
        public int Closed { get; set; }
    }
}
