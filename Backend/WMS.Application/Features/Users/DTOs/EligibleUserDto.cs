using System;

namespace WMS.Application.Features.Users.DTOs
{
    public class EligibleUserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string DesignationName { get; set; } = null!;
        public int DesignationLevel { get; set; }
    }
}
