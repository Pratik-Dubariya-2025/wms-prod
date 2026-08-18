using System;

namespace WMS.Application.Features.Departments.DTOs
{
    public class DepartmentListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int MemberCount { get; set; }
        public int DesignationCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
