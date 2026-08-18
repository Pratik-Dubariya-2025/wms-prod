using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WMS.Application.Features.Departments.Commands;
using WMS.Application.Features.Departments.Queries;

namespace WMS.API.Controllers
{
    [Route("api/departments")]
    [ApiController]
    [Authorize]
    public class DepartmentsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET /api/departments — List departments with member and designation counts.
        /// Permission: department.read
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDepartments([FromQuery] GetDepartmentsQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/departments/{id} — Get department details including members and designations.
        /// Permission: department.read
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDepartmentById(Guid id)
        {
            var query = new GetDepartmentByIdQuery { Id = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/departments — Create a new department.
        /// Permission: department.create
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PUT /api/departments/{id} — Update an existing department.
        /// Permission: department.update
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/departments/{id} — Soft delete a department.
        /// Permission: department.delete
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var command = new DeleteDepartmentCommand { Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        #region Designations

        /// <summary>
        /// POST /api/departments/{departmentId}/designations — Add a designation to a department.
        /// Permission: department.create
        /// </summary>
        [HttpPost("{departmentId:guid}/designations")]
        public async Task<IActionResult> CreateDesignation(Guid departmentId, [FromBody] CreateDesignationCommand command)
        {
            command.DepartmentId = departmentId;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PUT /api/departments/designations/{id} — Update a designation.
        /// Permission: department.update
        /// </summary>
        [HttpPut("designations/{id:guid}")]
        public async Task<IActionResult> UpdateDesignation(Guid id, [FromBody] UpdateDesignationCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/departments/designations/{id} — Soft delete a designation.
        /// Permission: department.delete
        /// </summary>
        [HttpDelete("designations/{id:guid}")]
        public async Task<IActionResult> DeleteDesignation(Guid id)
        {
            var command = new DeleteDesignationCommand { Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        #endregion
    }
}
