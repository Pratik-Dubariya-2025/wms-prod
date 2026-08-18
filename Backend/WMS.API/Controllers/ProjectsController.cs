using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Features.Projects.Commands;
using WMS.Application.Features.Projects.Queries;
using WMS.Application.Features.Tasks.Commands;
using WMS.Application.Features.Tasks.Queries;

namespace WMS.API.Controllers
{
    [Route("api/projects")]
    [ApiController]
    [Authorize]
    public class ProjectsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET /api/projects — List projects with pagination and search.
        /// Permission: project.read
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProjects([FromQuery] GetProjectsQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/projects/create-meta — Metadata for the create-project form
        /// (current user's department + selectable Tech Leads).
        /// Permission: any of project.create / project.update / project.delete.
        /// </summary>
        [HttpGet("create-meta")]
        public async Task<IActionResult> GetProjectCreateMeta()
        {
            var response = await _mediator.Send(new GetProjectCreateMetaQuery());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/projects/{id} — Get a single project's detail.
        /// Permission: project.read
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var query = new GetProjectByIdQuery { Id = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/projects — Create a new project.
        /// Permission: project.create
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PUT /api/projects/{id} — Update project metadata.
        /// Permission: project.update
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/projects/{id} — Delete a project.
        /// Permission: project.delete
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var command = new DeleteProjectCommand { Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/projects/{id}/members — Get project members.
        /// Permission: project.read
        /// </summary>
        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetProjectMembers(Guid id)
        {
            var query = new GetProjectMembersQuery { ProjectId = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/projects/{projectId}/members — Add a member to a project.
        /// Permission: project.update
        /// </summary>
        [HttpPost("{projectId:guid}/members")]
        public async Task<IActionResult> AddProjectMember(Guid projectId, [FromBody] AddProjectMemberCommand command)
        {
            command.ProjectId = projectId;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/projects/{projectId}/members/{userId} — Remove a member from a project.
        /// Permission: project.update
        /// </summary>
        [HttpDelete("{projectId:guid}/members/{userId:guid}")]
        public async Task<IActionResult> RemoveProjectMember(Guid projectId, Guid userId)
        {
            var command = new RemoveProjectMemberCommand { ProjectId = projectId, UserId = userId };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/projects/{projectId}/tasks — List tasks for a project.
        /// Permission: task.read
        /// </summary>
        [HttpGet("{projectId:guid}/tasks")]
        public async Task<IActionResult> GetProjectTasks(Guid projectId, [FromQuery] GetTasksQuery query)
        {
            query.ProjectId = projectId;
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/projects/{projectId}/tasks/create-meta — Get metadata for task creation (project members).
        /// Permission: task.create
        /// </summary>
        [HttpGet("{projectId:guid}/tasks/create-meta")]
        public async Task<IActionResult> GetTaskCreateMeta(Guid projectId)
        {
            var query = new GetTaskCreateMetaQuery { ProjectId = projectId };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/projects/{projectId}/tasks — Create a new task in a project.
        /// Permission: task.create
        /// </summary>
        [HttpPost("{projectId:guid}/tasks")]
        public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateTaskCommand command)
        {
            command.ProjectId = projectId;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PUT /api/projects/{projectId}/tasks/{id} — Update a task.
        /// Permission: task.update
        /// </summary>
        [HttpPut("{projectId:guid}/tasks/{id:guid}")]
        public async Task<IActionResult> UpdateTask(Guid projectId, Guid id, [FromBody] UpdateTaskCommand command)
        {
            command.ProjectId = projectId;
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PATCH /api/projects/{projectId}/tasks/{id}/status — Update task status.
        /// Permission: task.update
        /// </summary>
        [HttpPatch("{projectId:guid}/tasks/{id:guid}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid projectId, Guid id, [FromBody] UpdateTaskStatusCommand command)
        {
            command.ProjectId = projectId;
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/projects/{projectId}/tasks/{id} — Soft-delete a task.
        /// Permission: task.delete
        /// </summary>
        [HttpDelete("{projectId:guid}/tasks/{id:guid}")]
        public async Task<IActionResult> DeleteTask(Guid projectId, Guid id)
        {
            var command = new DeleteTaskCommand { ProjectId = projectId, Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/projects/{projectId}/tasks/{taskId}/time-logs — Log time against a task.
        /// Permission: timelog.create
        /// </summary>
        [HttpPost("{projectId:guid}/tasks/{taskId:guid}/time-logs")]
        public async Task<IActionResult> CreateTimeLog(Guid projectId, Guid taskId, [FromBody] CreateTimeLogCommand command)
        {
            command.TaskId = taskId;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/projects/{projectId}/tasks/{taskId}/time-logs — Get time logs for a task.
        /// Permission: timelog.read
        /// </summary>
        [HttpGet("{projectId:guid}/tasks/{taskId:guid}/time-logs")]
        public async Task<IActionResult> GetTimeLogs(Guid projectId, Guid taskId)
        {
            var query = new GetTimeLogsQuery { TaskId = taskId };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PUT /api/projects/{projectId}/tasks/{taskId}/time-logs/{logId}/approve — Approve a time log.
        /// Permission: timelog.approve
        /// </summary>
        [HttpPut("{projectId:guid}/tasks/{taskId:guid}/time-logs/{logId:guid}/approve")]
        public async Task<IActionResult> ApproveTimeLog(Guid projectId, Guid taskId, Guid logId)
        {
            var command = new ApproveTimeLogCommand { TaskId = taskId, TimeLogId = logId };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
