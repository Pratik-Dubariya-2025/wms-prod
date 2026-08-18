using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Leaves.Commands
{
    /// <summary>
    /// PATCH /api/leaves/{id}/approve
    /// Approve a pending leave request. Authorization follows the SRS 7.5 hierarchy
    /// via <see cref="LeaveApprovalPolicy"/>.
    /// Permission: leave.approve
    /// </summary>
    [RequirePermission(PermissionCodes.LeaveApprove)]
    public class ApproveLeaveRequestCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ApproveLeaveRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<ApproveLeaveRequestCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<bool>> Handle(
            ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var (leaveRequest, error) = await LeaveDecisionAuthorizer.LoadAndAuthorizeAsync(
                request.Id, _unitOfWork, _currentUserService, _policyEvaluationService);

            if (error != null)
            {
                return error;
            }

            leaveRequest!.Status = "Approved";
            leaveRequest.ApprovedById = _currentUserService.UserId!.Value;
            leaveRequest.ApprovedAt = DateTime.UtcNow;
            leaveRequest.RejectionReason = null;
            leaveRequest.ModifiedBy = _currentUserService.Username ?? "System";
            leaveRequest.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Leave request has been approved successfully.");
        }
    }
}
