namespace WMS.Application.Features.Leaves.DTOs
{
    /// <summary>
    /// DTO for displaying a leave request in lists.
    /// </summary>
    public class LeaveRequestListDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public string LeaveType { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal DaysCount { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = null!;
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
