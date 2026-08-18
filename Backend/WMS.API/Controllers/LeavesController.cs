using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Features.Leaves.Commands;
using WMS.Application.Features.Leaves.Queries;

namespace WMS.API.Controllers
{
    [Route("api/leaves")]
    [ApiController]
    [Authorize]
    public class LeavesController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET /api/leaves — List leave requests (scoped by role).
        /// Permission: leave.read
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLeaveRequests([FromQuery] GetLeaveRequestsQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/leaves — Submit a new leave request.
        /// Permission: leave.write
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PATCH /api/leaves/{id}/approve — Approve a pending leave request.
        /// Permission: leave.approve
        /// </summary>
        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> ApproveLeaveRequest(Guid id)
        {
            var response = await _mediator.Send(new ApproveLeaveRequestCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PATCH /api/leaves/{id}/reject — Reject a pending leave request (reason required).
        /// Permission: leave.approve
        /// </summary>
        [HttpPatch("{id:guid}/reject")]
        public async Task<IActionResult> RejectLeaveRequest(Guid id, [FromBody] RejectLeaveRequestCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
