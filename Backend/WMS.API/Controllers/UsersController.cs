using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Features.Users.Commands;
using WMS.Application.Features.Users.Queries;

namespace WMS.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UsersController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET /api/users — List users with pagination, search, and role/status filters.
        /// Permission: user.read
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/users/{id} — Get a single user's full profile.
        /// Permission: user.read
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var query = new GetUserByIdQuery { Id = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/users/invite-meta — Retrieve active departments, designations, and roles.
        /// Permission: user.create
        /// </summary>
        [HttpGet("invite-meta")]
        public async Task<IActionResult> GetInviteMeta()
        {
            var query = new GetInviteMetaQuery();
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/users/eligible-managers — Retrieve eligible managers for a department.
        /// Permission: user.read
        /// </summary>
        [HttpGet("eligible-managers")]
        public async Task<IActionResult> GetEligibleManagers([FromQuery] Guid departmentId)
        {
            var query = new GetEligibleManagersQuery { DepartmentId = departmentId };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/users/eligible-reporting-officers — Retrieve eligible reporting officers for a department.
        /// Permission: user.read
        /// </summary>
        [HttpGet("eligible-reporting-officers")]
        public async Task<IActionResult> GetEligibleReportingOfficers([FromQuery] Guid departmentId, [FromQuery] int invitedDesignationLevel, [FromQuery] Guid managerId)
        {
            var query = new GetEligibleReportingOfficersQuery { DepartmentId = departmentId, InvitedDesignationLevel = invitedDesignationLevel, ManagerId = managerId };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/users — Invite (create) a new user.
        /// Permission: user.create
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PUT /api/users/{id} — Update an existing user.
        /// Permission: user.update
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/users/{id} — Soft delete a user.
        /// Permission: user.delete
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var command = new DeleteUserCommand { Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/users/{id}/permissions — Retrieve effective permissions for a user.
        /// Permission: user.read
        /// </summary>
        [HttpGet("{id:guid}/permissions")]
        public async Task<IActionResult> GetUserPermissions(Guid id)
        {
            var query = new GetUserPermissionsQuery { UserId = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/users/{id}/roles — Retrieve assigned roles for a user.
        /// Permission: user.read
        /// </summary>
        [HttpGet("{id:guid}/roles")]
        public async Task<IActionResult> GetUserRoles(Guid id)
        {
            var query = new GetUserRolesQuery { UserId = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/users/{id}/permissions/overrides — Retrieve permission overrides for a user.
        /// Permission: user.read
        /// </summary>
        [HttpGet("{id:guid}/permissions/overrides")]
        public async Task<IActionResult> GetUserPermissionOverrides(Guid id)
        {
            var query = new GetUserPermissionOverridesQuery { UserId = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }
    }
}

