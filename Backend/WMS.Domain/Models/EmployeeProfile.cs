namespace WMS.Domain.Models
{
    /// <summary>
    /// HR-only extended profile data. Most users cannot see this.
    /// Maps to the SRS "employee_profiles" table (Section 4.5.1).
    /// </summary>
    public class EmployeeProfile : BaseModel
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal Salary { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankIfsc { get; set; }
        public string? PanNumber { get; set; }
        public string? AadhaarLast4 { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? BloodGroup { get; set; }
        public string? Address { get; set; }
    }
}
