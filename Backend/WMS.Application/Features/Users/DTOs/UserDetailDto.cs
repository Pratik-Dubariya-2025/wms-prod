namespace WMS.Application.Features.Users.DTOs
{
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }

        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;

        public Guid DesignationId { get; set; }
        public string DesignationName { get; set; } = null!;

        public List<UserRoleDto> Roles { get; set; } = [];
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

    }

    public class UserRoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }
}
