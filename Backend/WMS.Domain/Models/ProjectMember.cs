namespace WMS.Domain.Models
{
    /// <summary>
    /// Join entity linking a User to a Project.
    /// The user's Designation (via User.DesignationId) determines their role context within the project.
    /// </summary>
    public class ProjectMember : BaseModel
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
