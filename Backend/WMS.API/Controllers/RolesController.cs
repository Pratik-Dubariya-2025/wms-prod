using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Features.Roles.Commands;
using WMS.Application.Features.Roles.Queries;

namespace WMS.API.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class RolesController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET /api/roles — List roles with pagination and search.
        /// Permission: role.read
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles([FromQuery] GetRolesQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/roles — Create a new role.
        /// Permission: role.create
        /// </summary>
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PUT /api/roles/{id} — Update role metadata.
        /// Permission: role.update
        /// </summary>
        [HttpPut("roles/{id:guid}")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/roles/{id}/permissions — Retrieve permissions assigned to a role.
        /// Permission: role.read
        /// </summary>
        [HttpGet("roles/{id:guid}/permissions")]
        public async Task<IActionResult> GetRolePermissions(Guid id)
        {
            var query = new GetRolePermissionsQuery { RoleId = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/roles/{id}/permissions — Assign a permission to a role.
        /// Permission: role.update
        /// </summary>
        [HttpPost("roles/{id:guid}/permissions")]
        public async Task<IActionResult> AssignRolePermission(Guid id, [FromBody] AssignRolePermissionCommand command)
        {
            command.RoleId = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/roles/{id}/permissions/{permissionCode} — Remove a permission from a role.
        /// Permission: role.update
        /// </summary>
        [HttpDelete("roles/{id:guid}/permissions/{permissionCode}")]
        public async Task<IActionResult> RemoveRolePermission(Guid id, string permissionCode)
        {
            var command = new RemoveRolePermissionCommand { RoleId = id, PermissionCode = permissionCode };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/users/{id}/roles — Assign a role to a user.
        /// Permission: user.update
        /// </summary>
        [HttpPost("users/{id:guid}/roles")]
        public async Task<IActionResult> AssignUserRole(Guid id, [FromBody] AssignUserRoleCommand command)
        {
            command.UserId = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/users/{id}/roles/{roleId} — Remove a role from a user.
        /// Permission: user.update
        /// </summary>
        [HttpDelete("users/{id:guid}/roles/{roleId:guid}")]
        public async Task<IActionResult> RemoveUserRole(Guid id, Guid roleId)
        {
            var command = new RemoveUserRoleCommand { UserId = id, RoleId = roleId };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/users/{id}/permissions/override — Add or update a user permission override.
        /// Permission: user.update
        /// </summary>
        [HttpPost("users/{id:guid}/permissions/override")]
        public async Task<IActionResult> AddUserPermissionOverride(Guid id, [FromBody] AddUserPermissionOverrideCommand command)
        {
            command.UserId = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/users/{id}/permissions/override/{permissionCode} — Remove a user permission override.
        /// Permission: user.update
        /// </summary>
        [HttpDelete("users/{id:guid}/permissions/override/{permissionCode}")]
        public async Task<IActionResult> RemoveUserPermissionOverride(Guid id, string permissionCode)
        {
            var command = new RemoveUserPermissionOverrideCommand { UserId = id, PermissionCode = permissionCode };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
