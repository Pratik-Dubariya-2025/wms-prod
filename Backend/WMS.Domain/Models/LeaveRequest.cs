namespace WMS.Domain.Models
{
    /// <summary>
    /// Leave request submitted by employees and approved/rejected by managers.
    /// Maps to the SRS "leave_requests" table (Section 4.5.2).
    /// </summary>
    public class LeaveRequest : BaseModel
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        /// <summary>
        /// Annual, Sick, Casual, Maternity, Paternity, Unpaid
        /// </summary>
        public string LeaveType { get; set; } = null!;

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        /// <summary>
        /// Calculated days count; supports half days (e.g. 0.5, 1.5).
        /// </summary>
        public decimal DaysCount { get; set; }

        public string? Reason { get; set; }

        /// <summary>
        /// Pending, Approved, Rejected, Cancelled
        /// </summary>
        public string Status { get; set; } = "Pending";

        public Guid? ApprovedById { get; set; }
        public User? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public string? RejectionReason { get; set; }
    }
}
