using System;

namespace WMS.Application.Features.Roles.DTOs
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int Priority { get; set; }
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
    }
}
