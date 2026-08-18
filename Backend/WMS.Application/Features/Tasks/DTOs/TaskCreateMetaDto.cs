namespace WMS.Application.Features.Tasks.DTOs
{
    /// <summary>
    /// Metadata DTO returned for task creation form dropdowns 
    /// (projects, teams, assignable users).
    /// </summary>
    public class TaskCreateMetaDto
    {
        public List<TaskProjectLookup> Projects { get; set; } = [];
        public List<TaskTeamLookup> Teams { get; set; } = [];
        public List<TaskUserLookup> Users { get; set; } = [];
    }

    public class TaskProjectLookup
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid DepartmentId { get; set; }
    }

    public class TaskTeamLookup
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid DepartmentId { get; set; }
    }

    public class TaskUserLookup
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public Guid? TeamId { get; set; }
    }
}
