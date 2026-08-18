namespace WMS.Application.Features.Projects.DTOs
{
    /// <summary>
    /// DTO for project member listing.
    /// </summary>
    public class ProjectMemberDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DesignationName { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
    }
}
