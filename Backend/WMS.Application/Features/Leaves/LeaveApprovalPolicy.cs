using System.Linq.Expressions;
using WMS.Domain.Models;

namespace WMS.Application.Features.Leaves
{
    /// <summary>
    /// Roles snapshot of the user attempting to approve/reject a leave request.
    /// Built from <c>ICurrentUserService</c> so the policy has no dependency on HTTP/JWT.
    /// </summary>
    public sealed class LeaveApproverContext
    {
        public required Guid UserId { get; init; }
        public required Guid DepartmentId { get; init; }
        public bool IsAdmin { get; init; }
        public bool IsHR { get; init; }
        public bool IsAccounts { get; init; }
        public bool IsManager { get; init; }
        public bool IsTL { get; init; }
    }

    /// <summary>
    /// The org-structure facts about the leave requester needed to decide approval.
    /// </summary>
    public sealed class LeaveRequesterContext
    {
        public required Guid UserId { get; init; }
        public required Guid DepartmentId { get; init; }
        public string? DepartmentCode { get; init; }
        public Guid? ManagerId { get; init; }
        public Guid? ReportingOfficerId { get; init; }
        public bool IsTL { get; init; }
    }

    /// <summary>
    /// Single source of truth for the SRS 7.5 leave-approval hierarchy. Both the
    /// scoped list query and the approve/reject commands go through this class so the
    /// two can never drift apart.
    ///
    /// Rules (keyed on the REQUESTER's department):
    ///   • Admin and HR can approve any request (org-wide override per SRS 3.5.1 / role matrix).
    ///   • IT department:
    ///       - The requester's Tech Lead (their ReportingOfficer) approves developers under a TL.
    ///       - A Manager approves only people directly under them: developers with no TL
    ///         (ReportingOfficerId == null) or TLs themselves.
    ///       - A Manager may approve their own request.
    ///   • HR / Finance / Operations departments: the highest authority of that department
    ///     approves everyone in it. Highest-authority role per department:
    ///       HR → HR, FIN → ACCOUNTS, OPS → MANAGER. (The dept head may approve their own.)
    /// </summary>
    public static class LeaveApprovalPolicy
    {
        public const string DeptIT = "IT";
        public const string DeptHR = "HR";
        public const string DeptFinance = "FIN";
        public const string DeptOperations = "OPS";

        public const string RoleTL = "TL";

        /// <summary>
        /// Builds an approver context from a user's id, department and role codes.
        /// </summary>
        public static LeaveApproverContext BuildApprover(Guid userId, Guid departmentId, IEnumerable<string> roles)
        {
            var roleSet = roles as ICollection<string> ?? roles.ToList();
            return new LeaveApproverContext
            {
                UserId = userId,
                DepartmentId = departmentId,
                IsAdmin = roleSet.Contains("ADMIN"),
                IsHR = roleSet.Contains("HR"),
                IsAccounts = roleSet.Contains("ACCOUNTS"),
                IsManager = roleSet.Contains("MANAGER"),
                IsTL = roleSet.Contains(RoleTL),
            };
        }

        /// <summary>
        /// In-memory decision used by the approve/reject commands on a single loaded request.
        /// </summary>
        public static bool CanApprove(LeaveApproverContext approver, LeaveRequesterContext requester)
        {
            // Org-wide override.
            if (approver.IsAdmin || approver.IsHR)
            {
                return true;
            }

            switch (requester.DepartmentCode)
            {
                case DeptIT:
                    // Tech Lead approves developers who report to them.
                    if (approver.IsTL && requester.ReportingOfficerId == approver.UserId)
                    {
                        return true;
                    }

                    if (approver.IsManager)
                    {
                        // Manager approves people directly under them (no TL in between) or TLs.
                        bool directlyUnderManager =
                            requester.ManagerId == approver.UserId &&
                            (requester.ReportingOfficerId == null || requester.IsTL);

                        // Manager may approve their own request.
                        bool ownRequest = requester.UserId == approver.UserId;

                        if (directlyUnderManager || ownRequest)
                        {
                            return true;
                        }
                    }

                    return false;

                case DeptFinance:
                    return approver.IsAccounts && approver.DepartmentId == requester.DepartmentId;

                case DeptOperations:
                    return approver.IsManager && approver.DepartmentId == requester.DepartmentId;

                // HR department is already covered by the Admin/HR override above.
                default:
                    return false;
            }
        }

        /// <summary>
        /// SQL-translatable predicate mirroring <see cref="CanApprove"/>, used to scope the
        /// "approvals" listing to requests the current user is allowed to act on.
        /// Requires <c>User</c>, <c>User.Department</c> and <c>User.UserRoles.Role</c> to be queryable.
        /// </summary>
        public static Expression<Func<LeaveRequest, bool>> ApprovablePredicate(LeaveApproverContext approver)
        {
            var uid = approver.UserId;
            var deptId = approver.DepartmentId;
            var isAdmin = approver.IsAdmin;
            var isHR = approver.IsHR;
            var isAccounts = approver.IsAccounts;
            var isManager = approver.IsManager;
            var isTL = approver.IsTL;

            return lr =>
                isAdmin || isHR
                || (lr.User.Department.Code == DeptIT && (
                        (isTL && lr.User.ReportingOfficerId == uid)
                        || (isManager && (
                                (lr.User.ManagerId == uid &&
                                    (lr.User.ReportingOfficerId == null ||
                                     lr.User.UserRoles.Any(ur => ur.Role.Code == RoleTL && !ur.Role.IsDeleted)))
                                || lr.User.Id == uid))
                   ))
                || (lr.User.Department.Code == DeptFinance && isAccounts && lr.User.DepartmentId == deptId)
                || (lr.User.Department.Code == DeptOperations && isManager && lr.User.DepartmentId == deptId);
        }
    }
}
