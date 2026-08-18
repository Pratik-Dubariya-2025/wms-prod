using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Features.Policies.Commands;
using WMS.Application.Features.Policies.Queries;

namespace WMS.API.Controllers
{
    /// <summary>
    /// PBAC Policy management endpoints.
    /// These are purely additive — the existing RBAC system (RolesController) is untouched.
    /// </summary>
    [Route("api/policies")]
    [ApiController]
    [Authorize]
    public class PoliciesController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        // ────────────────────────────────────────────
        // Policy CRUD
        // ────────────────────────────────────────────

        /// <summary>
        /// GET /api/policies — List policies with pagination and filters.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPolicies([FromQuery] GetAccessPoliciesQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/policies/{id} — Get a single policy with all conditions and field restrictions.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPolicyById(Guid id)
        {
            var query = new GetPolicyByIdQuery { Id = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/policies — Create a new access policy (with optional conditions and field restrictions).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePolicy([FromBody] CreateAccessPolicyCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PUT /api/policies/{id} — Update policy metadata.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdateAccessPolicyCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/policies/{id} — Soft-delete a policy.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePolicy(Guid id)
        {
            var command = new DeleteAccessPolicyCommand { Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        // ────────────────────────────────────────────
        // Policy Conditions
        // ────────────────────────────────────────────

        /// <summary>
        /// POST /api/policies/{id}/conditions — Add a condition to a policy.
        /// </summary>
        [HttpPost("{id:guid}/conditions")]
        public async Task<IActionResult> AddCondition(Guid id, [FromBody] AddPolicyConditionCommand command)
        {
            command.PolicyId = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/policies/{id}/conditions/{conditionId} — Remove a condition from a policy.
        /// </summary>
        [HttpDelete("{id:guid}/conditions/{conditionId:guid}")]
        public async Task<IActionResult> RemoveCondition(Guid id, Guid conditionId)
        {
            var command = new RemovePolicyConditionCommand { PolicyId = id, ConditionId = conditionId };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        // ────────────────────────────────────────────
        // Field Restrictions (Column-level security)
        // ────────────────────────────────────────────

        /// <summary>
        /// POST /api/policies/{id}/fields — Add a field restriction to a policy.
        /// </summary>
        [HttpPost("{id:guid}/fields")]
        public async Task<IActionResult> AddFieldRestriction(Guid id, [FromBody] AddFieldRestrictionCommand command)
        {
            command.PolicyId = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// DELETE /api/policies/{id}/fields/{fieldId} — Remove a field restriction from a policy.
        /// </summary>
        [HttpDelete("{id:guid}/fields/{fieldId:guid}")]
        public async Task<IActionResult> RemoveFieldRestriction(Guid id, Guid fieldId)
        {
            var command = new RemoveFieldRestrictionCommand { PolicyId = id, FieldRestrictionId = fieldId };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        // ────────────────────────────────────────────
        // Utilities
        // ────────────────────────────────────────────

        /// <summary>
        /// GET /api/policies/modules — List all modules with their available CRUD actions (for the dropdown UI).
        /// </summary>
        [HttpGet("modules")]
        public async Task<IActionResult> GetModulesWithActions()
        {
            var query = new GetModulesWithActionsQuery();
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/policies/modules/{moduleCode}/attributes — List the "user.*"/"resource.*" bindings
        /// valid for a module, for the policy editor's attribute picker.
        /// </summary>
        [HttpGet("modules/{moduleCode}/attributes")]
        public async Task<IActionResult> GetPolicyAttributes(string moduleCode)
        {
            var query = new GetPolicyAttributesQuery { ModuleCode = moduleCode };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/policies/preview — Run a step-by-step dry-run evaluation of policies for a user.
        /// </summary>
        [HttpPost("preview")]
        public async Task<IActionResult> PreviewPolicyEffect([FromBody] PreviewPolicyEffectQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// GET /api/policies/export — Export all active access policies to a clean, portable JSON format.
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportPolicies()
        {
            var query = new ExportAccessPoliciesQuery();
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// POST /api/policies/import — Import access policies from a JSON file payload.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportPolicies([FromBody] ImportAccessPoliciesCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
