using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Features.Crm.Commands;
using WMS.Application.Features.Crm.Queries;

namespace WMS.API.Controllers
{
    [Route("api/crm/leads")]
    [ApiController]
    [Authorize]
    public class CrmController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET /api/crm/leads — List CRM leads (scoped by role).
        /// Permission: leads.read
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLeads([FromQuery] GetLeadsQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/crm/leads — BDA creates a new lead.
        /// Permission: leads.write
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLead([FromBody] CreateLeadCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PATCH /api/crm/leads/{id}/stage — Move lead through pipeline stages.
        /// Permission: leads.write
        /// </summary>
        [HttpPatch("{id:guid}/stage")]
        public async Task<IActionResult> UpdateLeadStage(Guid id, [FromBody] UpdateLeadStageCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
