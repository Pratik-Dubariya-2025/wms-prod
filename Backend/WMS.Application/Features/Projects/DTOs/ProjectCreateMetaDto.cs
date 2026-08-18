namespace WMS.Application.Features.Projects.DTOs
{
    /// <summary>
    /// Metadata for the Create Project form. The project is always created in the
    /// current user's department, so only that department is returned (read-only),
    /// along with the Tech Leads available to lead the project.
    /// </summary>
    public class ProjectCreateMetaDto
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public List<TeamLeadLookup> TeamLeads { get; set; } = [];
    }

    public class TeamLeadLookup
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }
    }
}
