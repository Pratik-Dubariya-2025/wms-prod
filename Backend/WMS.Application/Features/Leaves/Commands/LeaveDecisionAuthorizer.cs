using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Leaves.Commands
{
    /// <summary>
    /// Shared loading + hierarchy authorization for the approve and reject commands so
    /// both apply the exact same <see cref="LeaveApprovalPolicy"/> rules and pre-checks.
    /// </summary>
    internal static class LeaveDecisionAuthorizer
    {
        /// <summary>
        /// Loads the pending leave request and verifies the current user may act on it. Authorization
        /// is additive: the hardcoded <see cref="LeaveApprovalPolicy"/> hierarchy (Admin/HR/Accounts/
        /// Manager/TL) always keeps working exactly as before, and a PBAC policy for LEAVE_MGMT/Approve
        /// can grant approval rights to additional roles on top of it (e.g. an SSE approving their
        /// reporting team) without needing to touch or replace the hierarchy. Either channel saying
        /// yes is enough - neither can revoke what the other grants.
        /// On success returns the tracked entity and <c>error == null</c>; otherwise returns
        /// the populated failure response to return to the caller.
        /// </summary>
        public static async Task<(LeaveRequest? request, ApiResponse<bool>? error)> LoadAndAuthorizeAsync(
            Guid leaveRequestId,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IPolicyEvaluationService policyEvaluationService)
        {
            if (!currentUserService.UserId.HasValue)
            {
                return (null, ApiResponse<bool>.Failure("Unauthorized access.", null, 401));
            }

            var approverId = currentUserService.UserId.Value;

            var leaveRequest = await unitOfWork.LeaveRequest.IncludeAndGetFirstOrDefaultAsync<object>(
                where: lr => lr.Id == leaveRequestId && !lr.IsDeleted,
                includes: [lr => lr.User, lr => lr.User.Department]
            );

            if (leaveRequest == null)
            {
                return (null, ApiResponse<bool>.Failure("Leave request not found.", null, 404));
            }

            if (leaveRequest.Status != "Pending")
            {
                return (null, ApiResponse<bool>.Failure(
                    $"Leave request has already been {leaveRequest.Status.ToLower()}.", null, 400));
            }

            // The approver's own department is needed for the FIN/OPS highest-authority rules.
            var approverDepartmentId = await unitOfWork.User.GetFirstOrDefaultAsync(
                select: u => u.DepartmentId,
                where: u => u.Id == approverId && !u.IsDeleted);

            // Determine whether the requester is a Tech Lead (needed by the IT rules).
            var requesterRoleCodes = await unitOfWork.UserRole.GetAllAsync(
                select: ur => ur.Role.Code,
                where: ur => ur.UserId == leaveRequest.UserId && !ur.Role.IsDeleted);

            var approver = LeaveApprovalPolicy.BuildApprover(
                approverId, approverDepartmentId, currentUserService.Roles);

            var requester = new LeaveRequesterContext
            {
                UserId = leaveRequest.UserId,
                DepartmentId = leaveRequest.User.DepartmentId,
                DepartmentCode = leaveRequest.User.Department?.Code,
                ManagerId = leaveRequest.User.ManagerId,
                ReportingOfficerId = leaveRequest.User.ReportingOfficerId,
                IsTL = requesterRoleCodes.Contains(LeaveApprovalPolicy.RoleTL),
            };

            bool hierarchyAllows = LeaveApprovalPolicy.CanApprove(approver, requester);

            if (!hierarchyAllows)
            {
                var pbacResult = await policyEvaluationService.EvaluateAsync(approverId, "LEAVE_MGMT", "Approve", leaveRequest);
                bool pbacAllows = !pbacResult.NoPoliciesExist && pbacResult.IsAllowed;

                if (!pbacAllows)
                {
                    return (null, ApiResponse<bool>.Failure(
                        "You are not authorized to approve/reject this leave request.", null, 403));
                }
            }

            return (leaveRequest, null);
        }
    }
}
