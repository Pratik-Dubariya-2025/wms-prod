using WMS.Domain.Interfaces.Repositories;

namespace WMS.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }
        IRoleRepository Role { get; }
        IUserRoleRepository UserRole { get; }
        IModuleRepository Module { get; }
        IPermissionRepository Permission { get; }
        IRolePermissionRepository RolePermission { get; }
        IRefreshTokenRepository RefreshToken { get; }
        IDepartmentRepository Department { get; }
        IDesignationRepository Designation { get; }
        IForgotPasswordRepository ForgotPassword { get; }
        IUserPermissionOverrideRepository UserPermissionOverride { get; }
        IProjectRepository Project { get; }
        ITeamRepository Team { get; }
        ITaskItemRepository TaskItem { get; }
        IProjectMemberRepository ProjectMember { get; }
        ITimeLogRepository TimeLog { get; }
        IEmployeeProfileRepository EmployeeProfile { get; }
        ILeaveRequestRepository LeaveRequest { get; }
        ILeadRepository Lead { get; }
        IInvoiceRepository Invoice { get; }
        IAccessPolicyRepository AccessPolicy { get; }
        IPolicyConditionRepository PolicyCondition { get; }
        IFieldRestrictionRepository FieldRestriction { get; }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
