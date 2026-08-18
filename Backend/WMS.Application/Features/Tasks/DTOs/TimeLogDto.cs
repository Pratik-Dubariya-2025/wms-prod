namespace WMS.Application.Features.Tasks.DTOs
{
    /// <summary>
    /// DTO for displaying a time log entry in lists and detail views.
    /// </summary>
    public class TimeLogDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = null!;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public decimal LoggedHours { get; set; }
        public DateTime LogDate { get; set; }
        public string? Notes { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
