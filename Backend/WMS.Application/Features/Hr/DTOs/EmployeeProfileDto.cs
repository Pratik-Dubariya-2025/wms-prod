namespace WMS.Application.Features.Hr.DTOs
{
    /// <summary>
    /// DTO for displaying/editing an employee's extended HR profile.
    /// </summary>
    public class EmployeeProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;

        public decimal Salary { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankIfsc { get; set; }
        public string? PanNumber { get; set; }
        public string? AadhaarLast4 { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? BloodGroup { get; set; }
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
