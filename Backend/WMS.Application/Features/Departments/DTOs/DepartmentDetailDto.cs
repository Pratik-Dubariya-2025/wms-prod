using System;
using System.Collections.Generic;

namespace WMS.Application.Features.Departments.DTOs
{
    public class DepartmentDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DepartmentUserDto> Users { get; set; } = [];
        public List<DepartmentDesignationDto> Designations { get; set; } = [];
    }

    public class DepartmentUserDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? DesignationName { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
    }

    public class DepartmentDesignationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Level { get; set; }
        public bool IsActive { get; set; }
    }
}
