using System.Collections.Generic;
using WMS.Application.Common.Models;

namespace WMS.Application.Features.Users.Commands
{
    /// <summary>
    /// Strongly-typed mirror of the <c>@ErrorCode</c> values returned by
    /// <c>sp_InviteUser</c>. Keeping the codes in one place removes the magic
    /// integers that were previously duplicated between the stored procedure and
    /// the command handler.
    /// </summary>
    public enum InviteUserError
    {
        None = 0,
        DuplicateEmail = 1,
        DuplicateUsername = 2,
        DuplicateEmployeeCode = 3,
        DepartmentNotFound = 4,
        DesignationNotFound = 5,
        DesignationHierarchyViolation = 6,
        InvalidRoles = 7,
        RoleHierarchyViolation = 8,
        InvalidManager = 9,
        InvalidReportingOfficer = 10,
        ReportingOfficerRequired = 11,
        DatabaseError = 99
    }

    public static class InviteUserErrorMapper
    {
        private static readonly Dictionary<InviteUserError, (string Message, int StatusCode)> Map = new()
        {
            [InviteUserError.DuplicateEmail] = ("A user with this email already exists.", 409),
            [InviteUserError.DuplicateUsername] = ("A user with this username already exists.", 409),
            [InviteUserError.DuplicateEmployeeCode] = ("A user with this employee code already exists.", 409),
            [InviteUserError.DepartmentNotFound] = ("Department not found.", 400),
            [InviteUserError.DesignationNotFound] = ("Designation not found.", 400),
            [InviteUserError.DesignationHierarchyViolation] = ("Hierarchy violation: You cannot invite a user with a designation level equal to or higher than yours.", 403),
            [InviteUserError.InvalidRoles] = ("One or more invited roles are invalid.", 400),
            [InviteUserError.RoleHierarchyViolation] = ("Hierarchy violation: You cannot invite a user with roles that have a rank equal to or higher than yours.", 403),
            [InviteUserError.InvalidManager] = ("Invalid Manager: the manager must be the highest-level (department head) active user in the same department.", 400),
            [InviteUserError.InvalidReportingOfficer] = ("Invalid Reporting Officer: must be in the same department, rank above the invited user, and not outrank the manager.", 400),
            [InviteUserError.ReportingOfficerRequired] = ("A Reporting Officer (Team Lead, or the Manager when no Team Lead exists) is required for this designation.", 400),
            [InviteUserError.DatabaseError] = ("Failed to invite user due to a database error.", 500)
        };

        public static ApiResponse<TResult> ToFailure<TResult>(InviteUserError error)
        {
            var (message, statusCode) = Map.TryGetValue(error, out var entry)
                ? entry
                : ("Failed to invite user.", 500);

            return ApiResponse<TResult>.Failure(message, statusCode: statusCode);
        }
    }
}
