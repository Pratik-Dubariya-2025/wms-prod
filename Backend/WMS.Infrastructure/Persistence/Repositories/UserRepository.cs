using Microsoft.EntityFrameworkCore;
using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
    {
        public async Task<User?> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                 .Include(u => u.Department)
                 .Include(u => u.Designation)
                 .Include(u => u.UserRoles)
                     .ThenInclude(ur => ur.Role)
                         .ThenInclude(r => r.RolePermissions)
                             .ThenInclude(rp => rp.Permission)
                 .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<List<Guid>> GetSubordinateIdsAsync(Guid managerId, CancellationToken cancellationToken = default)
        {
            return await context.Database.SqlQuery<Guid>($@"
WITH Subordinates AS (
    SELECT Id FROM Users WHERE ManagerId = {managerId} AND IsDeleted = 0
    UNION ALL
    SELECT u.Id FROM Users u
    INNER JOIN Subordinates s ON u.ManagerId = s.Id
    WHERE u.IsDeleted = 0
)
SELECT Id AS ""Value"" FROM Subordinates
OPTION (MAXRECURSION 100)")
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Guid>> GetReportingTeamIdsAsync(Guid officerId, CancellationToken cancellationToken = default)
        {
            return await context.Database.SqlQuery<Guid>($@"
WITH ReportingTeam AS (
    SELECT Id FROM Users WHERE ReportingOfficerId = {officerId} AND IsDeleted = 0
    UNION ALL
    SELECT u.Id FROM Users u
    INNER JOIN ReportingTeam rt ON u.ReportingOfficerId = rt.Id
    WHERE u.IsDeleted = 0
)
SELECT Id AS ""Value"" FROM ReportingTeam
OPTION (MAXRECURSION 100)")
                .ToListAsync(cancellationToken);
        }

        public async Task<List<User>> GetEligibleManagersAsync(
            Guid departmentId, CancellationToken cancellationToken = default)
        {
            var deptUsers = DbSet.Where(u =>
                u.DepartmentId == departmentId && u.IsActive && !u.IsDeleted);

            return await deptUsers
                .Where(u => u.Designation.Level == deptUsers.Max(x => x.Designation.Level))
                .Include(u => u.Designation)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<User>> GetEligibleReportingOfficersAsync(
            Guid departmentId, int invitedDesignationLevel, Guid managerId,
            CancellationToken cancellationToken = default)
        {
            // Reporting officers are the Tech Leads that report to this manager,
            // plus the manager themselves as a fallback for the "no Team Lead" case.
            return await DbSet
                .Where(u =>
                    u.DepartmentId == departmentId
                    && u.IsActive
                    && !u.IsDeleted
                    && u.Designation.Level > invitedDesignationLevel
                    && (u.ManagerId == managerId || u.Id == managerId))
                .Include(u => u.Designation)
                .ToListAsync(cancellationToken);
        }

        public async Task<(Guid? UserId, int ErrorCode)> InviteUserViaSpAsync(
            string employeeCode,
            string firstName,
            string lastName,
            string email,
            string username,
            string passwordHash,
            string? phoneNumber,
            Guid departmentId,
            Guid designationId,
            string createdBy,
            List<Guid> roleIds,
            Guid currentUserId,
            Guid? managerId,
            Guid? reportingOfficerId,
            CancellationToken cancellationToken = default)
        {
            var employeeCodeParam = new Microsoft.Data.SqlClient.SqlParameter("@EmployeeCode", employeeCode);
            var firstNameParam = new Microsoft.Data.SqlClient.SqlParameter("@FirstName", firstName);
            var lastNameParam = new Microsoft.Data.SqlClient.SqlParameter("@LastName", lastName);
            var emailParam = new Microsoft.Data.SqlClient.SqlParameter("@Email", email);
            var usernameParam = new Microsoft.Data.SqlClient.SqlParameter("@Username", username);
            var passwordHashParam = new Microsoft.Data.SqlClient.SqlParameter("@PasswordHash", passwordHash);
            var phoneNumberParam = new Microsoft.Data.SqlClient.SqlParameter("@PhoneNumber", (object?)phoneNumber ?? DBNull.Value);
            var departmentIdParam = new Microsoft.Data.SqlClient.SqlParameter("@DepartmentId", departmentId);
            var designationIdParam = new Microsoft.Data.SqlClient.SqlParameter("@DesignationId", designationId);
            var createdByParam = new Microsoft.Data.SqlClient.SqlParameter("@CreatedBy", createdBy);
            
            string roleIdsJson = System.Text.Json.JsonSerializer.Serialize(roleIds);
            var roleIdsJsonParam = new Microsoft.Data.SqlClient.SqlParameter("@RoleIdsJson", roleIdsJson);
            var currentUserIdParam = new Microsoft.Data.SqlClient.SqlParameter("@CurrentUserId", currentUserId);
            var managerIdParam = new Microsoft.Data.SqlClient.SqlParameter("@ManagerId", (object?)managerId ?? DBNull.Value);
            var reportingOfficerIdParam = new Microsoft.Data.SqlClient.SqlParameter("@ReportingOfficerId", (object?)reportingOfficerId ?? DBNull.Value);

            var newUserIdParam = new Microsoft.Data.SqlClient.SqlParameter("@NewUserId", System.Data.SqlDbType.UniqueIdentifier)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            var errorCodeParam = new Microsoft.Data.SqlClient.SqlParameter("@ErrorCode", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await context.Database.ExecuteSqlRawAsync(
                "EXEC sp_InviteUser @EmployeeCode, @FirstName, @LastName, @Email, @Username, @PasswordHash, @PhoneNumber, @DepartmentId, @DesignationId, @CreatedBy, @RoleIdsJson, @CurrentUserId, @ManagerId, @ReportingOfficerId, @NewUserId OUTPUT, @ErrorCode OUTPUT",
                new object[] { 
                    employeeCodeParam, firstNameParam, lastNameParam, emailParam, usernameParam, 
                    passwordHashParam, phoneNumberParam, departmentIdParam, designationIdParam, 
                    createdByParam, roleIdsJsonParam, currentUserIdParam, managerIdParam, reportingOfficerIdParam,
                    newUserIdParam, errorCodeParam 
                }, 
                cancellationToken);

            Guid? newUserId = newUserIdParam.Value == DBNull.Value ? null : (Guid)newUserIdParam.Value;
            int errorCode = errorCodeParam.Value == DBNull.Value ? 0 : (int)errorCodeParam.Value;

            return (newUserId, errorCode);
        }
    }
}
