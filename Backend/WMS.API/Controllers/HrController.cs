using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Features.Hr.Commands;
using WMS.Application.Features.Hr.Queries;

namespace WMS.API.Controllers
{
    [Route("api/hr")]
    [ApiController]
    [Authorize]
    public class HrController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET /api/hr/profiles/{userId} — Get extended HR profile (salary, bank details, etc.).
        /// Permission: salary.read (HR and Admin only)
        /// </summary>
        [HttpGet("profiles/{userId:guid}")]
        public async Task<IActionResult> GetEmployeeProfile(Guid userId)
        {
            var query = new GetEmployeeProfileQuery { UserId = userId };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PUT /api/hr/profiles/{userId} — Create or update HR profile.
        /// Permission: salary.write (HR and Admin only)
        /// </summary>
        [HttpPut("profiles/{userId:guid}")]
        public async Task<IActionResult> UpdateEmployeeProfile(Guid userId, [FromBody] UpdateEmployeeProfileCommand command)
        {
            command.UserId = userId;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
