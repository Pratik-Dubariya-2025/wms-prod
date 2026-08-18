using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;

namespace WMS.Application.Features.Policies.Queries
{
    public class PolicyAttributesDto
    {
        /// <summary>
        /// Curated "user.*" bindings safe to use in any policy condition/row filter - the acting
        /// user's own scalar attributes plus the "SubordinateIds" hierarchy primitive.
        /// </summary>
        public List<string> UserAttributes { get; set; } = [];

        /// <summary>
        /// "resource.*" bindings valid for this specific module, sourced from the module's
        /// registered entity so the policy editor can't offer a field that doesn't exist.
        /// Empty if the module isn't registered in the PBAC resource registry.
        /// </summary>
        public List<string> ResourceAttributes { get; set; } = [];

        public bool ModuleSupported { get; set; }
    }

    /// <summary>
    /// Backs the policy editor's attribute pickers so admins choose real fields instead of
    /// hand-typing property names that may not exist.
    /// </summary>
    [RequirePermission(PermissionCodes.PolicyRead)]
    public class GetPolicyAttributesQuery : IRequest<ApiResponse<PolicyAttributesDto>>
    {
        public string ModuleCode { get; set; } = null!;
    }

    public class GetPolicyAttributesQueryHandler(IPolicyResourceRegistry resourceRegistry)
        : IRequestHandler<GetPolicyAttributesQuery, ApiResponse<PolicyAttributesDto>>
    {
        private readonly IPolicyResourceRegistry _resourceRegistry = resourceRegistry;

        // Curated, not reflected off User: only attributes safe/meaningful for policy authoring,
        // excluding things like PasswordHash, MfaSecret, RefreshTokens, etc.
        private static readonly List<string> UserAttributes =
        [
            "Id",
            "DepartmentId",
            "DesignationId",
            "TeamId",
            "ManagerId",
            "ReportingOfficerId",
            "SubordinateIds",
            "ReportingTeamIds"
        ];

        public Task<ApiResponse<PolicyAttributesDto>> Handle(GetPolicyAttributesQuery request, CancellationToken cancellationToken)
        {
            var moduleCode = request.ModuleCode?.Trim() ?? "";
            bool supported = _resourceRegistry.IsModuleSupported(moduleCode);

            var dto = new PolicyAttributesDto
            {
                UserAttributes = UserAttributes,
                ResourceAttributes = supported ? [.. _resourceRegistry.GetSupportedAttributes(moduleCode)] : [],
                ModuleSupported = supported
            };

            return Task.FromResult(ApiResponse<PolicyAttributesDto>.Success(dto, "Policy attributes retrieved successfully."));
        }
    }
}
