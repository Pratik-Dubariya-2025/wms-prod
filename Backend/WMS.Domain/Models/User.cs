namespace WMS.Domain.Models
{
    public class User : BaseModel
    {
        public string EmployeeCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFirstTimeLogin { get; set; } = true;

        public int AccessFailedCount { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public Guid DesignationId { get; set; }
        public Designation Designation { get; set; } = null!;

        // Reports-to hierarchy (SRS users.manager_id)
        public Guid? ManagerId { get; set; }
        public User? Manager { get; set; }

        // Daily reporting officer — assigns tasks, approves leaves at team level
        public Guid? ReportingOfficerId { get; set; }
        public User? ReportingOfficer { get; set; }

        // Team membership for row-level scoping (SRS users.team_id)
        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }

        public bool IsMfaEnabled { get; set; } = false;
        public string? MfaSecret { get; set; }

        // Many-to-Many: A user can have multiple roles
        public List<UserRole> UserRoles { get; set; } = [];

        public List<ForgotPassword> ForgotPasswords { get; set; } = [];

        public List<RefreshToken> RefreshTokens { get; set; } = [];

        public List<UserPermissionOverride> UserPermissionOverrides { get; set; } = [];
    }
}
