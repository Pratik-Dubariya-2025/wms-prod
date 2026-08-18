using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Leaves.Commands
{
    /// <summary>
    /// PATCH /api/leaves/{id}/reject
    /// Reject a pending leave request with a mandatory reason. Authorization follows the
    /// SRS 7.5 hierarchy via <see cref="LeaveApprovalPolicy"/>.
    /// Permission: leave.approve
    /// </summary>
    [RequirePermission(PermissionCodes.LeaveApprove)]
    public class RejectLeaveRequestCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class RejectLeaveRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<RejectLeaveRequestCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<bool>> Handle(
            RejectLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return ApiResponse<bool>.Failure(
                    "Rejection reason is required when rejecting a leave request.", null, 400);
            }

            var (leaveRequest, error) = await LeaveDecisionAuthorizer.LoadAndAuthorizeAsync(
                request.Id, _unitOfWork, _currentUserService, _policyEvaluationService);

            if (error != null)
            {
                return error;
            }

            leaveRequest!.Status = "Rejected";
            leaveRequest.ApprovedById = _currentUserService.UserId!.Value;
            leaveRequest.ApprovedAt = DateTime.UtcNow;
            leaveRequest.RejectionReason = request.RejectionReason.Trim();
            leaveRequest.ModifiedBy = _currentUserService.Username ?? "System";
            leaveRequest.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Leave request has been rejected successfully.");
        }
    }
}
