using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Common.Policies;
using WMS.Application.Features.Leaves.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Leaves.Queries
{
    /// <summary>
    /// GET /api/leaves
    /// List leave requests. Three modes, mutually exclusive (ApprovalsOnly wins if both are set):
    ///   - Default ("My Leaves"): ALWAYS only the caller's own requests. This is a strict self-view by
    ///     definition and is never widened by policy - the standard pattern (as in most HRMS/leave
    ///     tools) is to keep "my records" permanently self-scoped and put any broader visibility in a
    ///     separate, explicitly-labeled view (see TeamOnly below), rather than have a tab named "My
    ///     Leaves" silently start showing other people's data because of an admin-configured policy.
    ///   - TeamOnly ("Team Leaves"): scope is governed ENTIRELY by PBAC - no self-view, no hardcoded
    ///     fallback. If no policy governs LEAVE_MGMT/read for the caller, this returns nothing (there
    ///     is no notion of "team" without an explicit policy granting it). This is the view a policy
    ///     like "{user.SubordinateIds}" or "{user.ReportingTeamIds}" is meant to populate.
    ///   - ApprovalsOnly ("Approvals"): requests the caller is allowed to approve. Additive: always
    ///     includes everything <see cref="LeaveApprovalPolicy"/> (the SRS 7.5 hierarchy) grants, plus
    ///     anything a PBAC policy for LEAVE_MGMT/Approve additionally grants on top (e.g. an SSE
    ///     approving their reporting team) - see <see cref="Commands.LeaveDecisionAuthorizer"/> for the
    ///     matching single-record check the approve/reject commands enforce.
    /// Permission: leave.read
    /// </summary>
    [RequirePermission(PermissionCodes.LeaveRead)]
    public class GetLeaveRequestsQuery : IRequest<ApiResponse<PaginatedResult<LeaveRequestListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? LeaveType { get; set; }
        public bool ApprovalsOnly { get; set; } = false;
        public bool TeamOnly { get; set; } = false;
    }

    public class GetLeaveRequestsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPolicyEvaluationService policyEvaluationService)
        : IRequestHandler<GetLeaveRequestsQuery, ApiResponse<PaginatedResult<LeaveRequestListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPolicyEvaluationService _policyEvaluationService = policyEvaluationService;

        public async Task<ApiResponse<PaginatedResult<LeaveRequestListDto>>> Handle(
            GetLeaveRequestsQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            if (currentUserId == null)
            {
                return ApiResponse<PaginatedResult<LeaveRequestListDto>>.Failure("Unauthorized access.", null, 401);
            }

            var currentUser = await _unitOfWork.User.IncludeAndGetFirstOrDefaultAsync<object>(
                u => u.Id == currentUserId && !u.IsDeleted,
                includes: [u => u.Department, u => u.Designation]
            );

            if (currentUser == null)
            {
                return ApiResponse<PaginatedResult<LeaveRequestListDto>>.Failure("User not found.", null, 404);
            }

            var approver = LeaveApprovalPolicy.BuildApprover(
                currentUserId.Value, currentUser.DepartmentId, _currentUserService.Roles);

            // TeamOnly mode consults PBAC as its entire scope - see the class-level doc comment for why
            // this is a separate mode rather than something the default "My Leaves" view falls into.
            bool teamPbacGoverned = false;
            System.Linq.Expressions.Expression<Func<WMS.Domain.Models.LeaveRequest, bool>>? pbacFilter = null;
            if (request.TeamOnly && !request.ApprovalsOnly)
            {
                var (governed, filter) = await PbacRowScope.ResolveAsync<WMS.Domain.Models.LeaveRequest>(
                    _policyEvaluationService, currentUserId, "LEAVE_MGMT", "read");
                teamPbacGoverned = governed;
                pbacFilter = filter;
            }

            // ApprovalsOnly mode: the hierarchy predicate stays the baseline; a PBAC policy for
            // LEAVE_MGMT/Approve additively widens it (never narrows it - see the class doc comment).
            System.Linq.Expressions.Expression<Func<WMS.Domain.Models.LeaveRequest, bool>> approvablePredicate =
                LeaveApprovalPolicy.ApprovablePredicate(approver);
            if (request.ApprovalsOnly)
            {
                var (approveGoverned, approveFilter) = await PbacRowScope.ResolveAsync<WMS.Domain.Models.LeaveRequest>(
                    _policyEvaluationService, currentUserId, "LEAVE_MGMT", "approve");
                if (approveGoverned)
                {
                    approvablePredicate = approveFilter != null
                        ? ExpressionCombinators.Or(approvablePredicate, approveFilter)
                        : (lr => true); // an unrestricted Allow policy matched - every record
                }
            }

            WMS.Domain.Common.Models.PaginationDTO<LeaveRequestListDto> paginationDto = new()
            {
                PageNumber = request.PageNumber,
                ItemsPerPage = request.PageSize,
                Search = request.Search
            };

            var result = await _unitOfWork.LeaveRequest.GetPaginatedList(
                select: lr => new LeaveRequestListDto
                {
                    Id = lr.Id,
                    UserId = lr.UserId,
                    UserName = lr.User.FirstName + " " + lr.User.LastName,
                    UserEmail = lr.User.Email,
                    EmployeeCode = lr.User.EmployeeCode,
                    LeaveType = lr.LeaveType,
                    FromDate = lr.FromDate,
                    ToDate = lr.ToDate,
                    DaysCount = lr.DaysCount,
                    Reason = lr.Reason,
                    Status = lr.Status,
                    ApprovedByName = lr.ApprovedBy != null
                        ? lr.ApprovedBy.FirstName + " " + lr.ApprovedBy.LastName
                        : null,
                    ApprovedAt = lr.ApprovedAt,
                    RejectionReason = lr.RejectionReason,
                    CreatedAt = lr.CreatedAt
                },
                paginationDto,
                where: query =>
                {
                    if (request.ApprovalsOnly)
                    {
                        // Scope to requests this user is allowed to approve - hierarchy rules plus
                        // any additive PBAC grant, matching what the approve/reject commands enforce.
                        query = query.Where(approvablePredicate);
                    }
                    else if (request.TeamOnly)
                    {
                        if (!teamPbacGoverned)
                        {
                            // No policy grants this caller any team visibility - "Team Leaves" is
                            // meaningless without one, so show nothing rather than guessing a fallback.
                            query = query.Where(lr => false);
                        }
                        else if (pbacFilter != null)
                        {
                            query = query.Where(pbacFilter);
                        }
                        // else: an unrestricted Allow policy matched - every record, no filter needed.
                    }
                    else
                    {
                        // "My Leaves" - always strictly the caller's own requests, never widened by
                        // policy (see the class-level doc comment).
                        query = query.Where(lr => lr.UserId == currentUserId);
                    }

                    // Text search across user name
                    if (!string.IsNullOrWhiteSpace(request.Search))
                    {
                        string search = request.Search.ToLower();
                        query = query.Where(lr =>
                            (lr.User.FirstName + " " + lr.User.LastName).ToLower().Contains(search) ||
                            lr.User.EmployeeCode.ToLower().Contains(search) ||
                            (lr.Reason != null && lr.Reason.ToLower().Contains(search)));
                    }

                    // Status filter
                    if (!string.IsNullOrWhiteSpace(request.Status))
                    {
                        query = query.Where(lr => lr.Status == request.Status);
                    }

                    // LeaveType filter
                    if (!string.IsNullOrWhiteSpace(request.LeaveType))
                    {
                        query = query.Where(lr => lr.LeaveType == request.LeaveType);
                    }

                    // Exclude soft-deleted
                    query = query.Where(lr => !lr.IsDeleted);

                    return query;
                }
            );

            PaginatedResult<LeaveRequestListDto> paginatedResult = PaginatedResult<LeaveRequestListDto>.Create(
                result.Records,
                result.TotalRecords,
                result.PageNumber,
                result.ItemsPerPage
            );

            return ApiResponse<PaginatedResult<LeaveRequestListDto>>.Success(paginatedResult);
        }
    }
}
