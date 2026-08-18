namespace WMS.Application.Features.Projects.DTOs
{
    /// <summary>
    /// Lightweight DTO used in paginated project lists.
    /// </summary>
    public class ProjectListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OwnerName { get; set; } = null!;
        public string? TeamLeadName { get; set; }
        public string DepartmentName { get; set; } = null!;
        public int MemberCount { get; set; }
        public int TaskCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
