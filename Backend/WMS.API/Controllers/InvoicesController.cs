using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Features.Invoices.Commands;
using WMS.Application.Features.Invoices.Queries;

namespace WMS.API.Controllers
{
    [Route("api/accounts/invoices")]
    [ApiController]
    [Authorize]
    public class InvoicesController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET /api/accounts/invoices — List financial invoices.
        /// Permission: invoices.read
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInvoices([FromQuery] GetInvoicesQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/accounts/invoices — Create invoice from closed crm deal.
        /// Permission: invoices.write
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
