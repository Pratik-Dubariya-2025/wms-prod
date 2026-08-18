using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.Infrastructure.Persistence.Repositories;

namespace WMS.Infrastructure.Persistence
{
    public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
    {
        private IUserRepository? _userRepository;
        private IRoleRepository? _roleRepository;
        private IUserRoleRepository? _userRoleRepository;
        private IModuleRepository? _moduleRepository;
        private IPermissionRepository? _permissionRepository;
        private IRolePermissionRepository? _rolePermissionRepository;
        private IRefreshTokenRepository? _refreshTokenRepository;
        private IDepartmentRepository? _departmentRepository;
        private IDesignationRepository? _designationRepository;
        private IForgotPasswordRepository? _forgotPasswordRepository;
        private IUserPermissionOverrideRepository? _userPermissionOverrideRepository;
        private IProjectRepository? _projectRepository;
        private ITeamRepository? _teamRepository;
        private ITaskItemRepository? _taskItemRepository;
        private IProjectMemberRepository? _projectMemberRepository;
        private ITimeLogRepository? _timeLogRepository;
        private IEmployeeProfileRepository? _employeeProfileRepository;
        private ILeaveRequestRepository? _leaveRequestRepository;
        private ILeadRepository? _leadRepository;
        private IInvoiceRepository? _invoiceRepository;
        private IAccessPolicyRepository? _accessPolicyRepository;
        private IPolicyConditionRepository? _policyConditionRepository;
        private IFieldRestrictionRepository? _fieldRestrictionRepository;

        private readonly AppDbContext _dbContext = dbContext;

        IUserRepository IUnitOfWork.User => _userRepository ??= new UserRepository(_dbContext);
        IRoleRepository IUnitOfWork.Role => _roleRepository ??= new RoleRepository(_dbContext);
        IUserRoleRepository IUnitOfWork.UserRole => _userRoleRepository ??= new UserRoleRepository(_dbContext);
        IModuleRepository IUnitOfWork.Module => _moduleRepository ??= new ModuleRepository(_dbContext);
        IPermissionRepository IUnitOfWork.Permission => _permissionRepository ??= new PermissionRepository(_dbContext);
        IRolePermissionRepository IUnitOfWork.RolePermission => _rolePermissionRepository ??= new RolePermissionRepository(_dbContext);
        IRefreshTokenRepository IUnitOfWork.RefreshToken => _refreshTokenRepository ??= new RefreshTokenRepository(_dbContext);
        IDepartmentRepository IUnitOfWork.Department => _departmentRepository ??= new DepartmentRepository(_dbContext);
        IDesignationRepository IUnitOfWork.Designation => _designationRepository ??= new DesignationRepository(_dbContext);
        IForgotPasswordRepository IUnitOfWork.ForgotPassword => _forgotPasswordRepository ??= new ForgotPasswordRepository(_dbContext);
        IUserPermissionOverrideRepository IUnitOfWork.UserPermissionOverride => _userPermissionOverrideRepository ??= new UserPermissionOverrideRepository(_dbContext);
        IProjectRepository IUnitOfWork.Project => _projectRepository ??= new ProjectRepository(_dbContext);
        ITeamRepository IUnitOfWork.Team => _teamRepository ??= new TeamRepository(_dbContext);
        ITaskItemRepository IUnitOfWork.TaskItem => _taskItemRepository ??= new TaskItemRepository(_dbContext);
        IProjectMemberRepository IUnitOfWork.ProjectMember => _projectMemberRepository ??= new ProjectMemberRepository(_dbContext);
        ITimeLogRepository IUnitOfWork.TimeLog => _timeLogRepository ??= new TimeLogRepository(_dbContext);
        IEmployeeProfileRepository IUnitOfWork.EmployeeProfile => _employeeProfileRepository ??= new EmployeeProfileRepository(_dbContext);
        ILeaveRequestRepository IUnitOfWork.LeaveRequest => _leaveRequestRepository ??= new LeaveRequestRepository(_dbContext);
        ILeadRepository IUnitOfWork.Lead => _leadRepository ??= new LeadRepository(_dbContext);
        IInvoiceRepository IUnitOfWork.Invoice => _invoiceRepository ??= new InvoiceRepository(_dbContext);
        IAccessPolicyRepository IUnitOfWork.AccessPolicy => _accessPolicyRepository ??= new AccessPolicyRepository(_dbContext);
        IPolicyConditionRepository IUnitOfWork.PolicyCondition => _policyConditionRepository ??= new PolicyConditionRepository(_dbContext);
        IFieldRestrictionRepository IUnitOfWork.FieldRestriction => _fieldRestrictionRepository ??= new FieldRestrictionRepository(_dbContext);

        async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
