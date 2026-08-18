using System;
using System.Collections.Generic;
using WMS.Application.Features.Roles.DTOs;

namespace WMS.Application.Features.Users.DTOs
{
    public class InviteMetaDto
    {
        public List<DepartmentDto> Departments { get; set; } = [];
        public List<DesignationDto> Designations { get; set; } = [];
        public List<RoleDto> Roles { get; set; } = [];
    }

    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class DesignationDto
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public string Name { get; set; } = null!;
        public int Level { get; set; }
    }
}
