using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;

namespace WMS.Application.Features.Policies.Queries
{
    [RequirePermission(PermissionCodes.PolicyRead)]
    public class PreviewPolicyEffectQuery : IRequest<ApiResponse<PolicyPreviewResult>>
    {
        public Guid UserId { get; set; }
        public string ModuleCode { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? ResourceContextJson { get; set; }
    }

    public class PreviewPolicyEffectQueryHandler(IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<PreviewPolicyEffectQuery, ApiResponse<PolicyPreviewResult>>
    {
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<PolicyPreviewResult>> Handle(PreviewPolicyEffectQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ModuleCode))
            {
                return ApiResponse<PolicyPreviewResult>.Failure("ModuleCode is required.", null, 400);
            }
            if (string.IsNullOrWhiteSpace(request.Action))
            {
                return ApiResponse<PolicyPreviewResult>.Failure("Action is required.", null, 400);
            }

            var preview = await _policyEvaluationService.PreviewEvaluationAsync(
                request.UserId,
                request.ModuleCode.Trim(),
                request.Action.Trim(),
                request.ResourceContextJson
            );

            return ApiResponse<PolicyPreviewResult>.Success(preview, "Policy preview generated successfully.");
        }
    }
}
