namespace WMS.Domain.Models
{
    /// <summary>
    /// Tracks time logged by users against tasks.
    /// Maps to the SRS "time_logs" table (Section 4.4.3).
    /// </summary>
    public class TimeLog : BaseModel
    {
        public Guid TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal LoggedHours { get; set; }
        public DateTime LogDate { get; set; }
        public string? Notes { get; set; }

        public bool IsApproved { get; set; }
        public Guid? ApprovedById { get; set; }
        public User? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
