using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Users.Commands
{
    [RequirePermission(PermissionCodes.UserCreate)]
    public class InviteUserCommand : IRequest<ApiResponse<Guid>>
    {
        public string EmployeeCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid DesignationId { get; set; }
        public List<Guid> RoleIds { get; set; } = [];
        public Guid? ManagerId { get; set; }
        public Guid? ReportingOfficerId { get; set; }
    }

    public class InviteUserCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IWelcomeEmailService welcomeEmailService,
        IUserHierarchyService userHierarchyService)
        : IRequestHandler<InviteUserCommand, ApiResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IWelcomeEmailService _welcomeEmailService = welcomeEmailService;
        private readonly IUserHierarchyService _userHierarchyService = userHierarchyService;

        public async Task<ApiResponse<Guid>> Handle(
            InviteUserCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<Guid>.Failure("Unauthorized access.", statusCode: 401);
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            string createdBy = _currentUserService.Username ?? "System";

            // The stored procedure performs all validation, the user insert, role
            // assignment, and team derivation/creation in a single atomic transaction.
            var (newUserId, errorCode) = await _unitOfWork.User.InviteUserViaSpAsync(
                request.EmployeeCode,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Username,
                passwordHash,
                request.PhoneNumber,
                request.DepartmentId,
                request.DesignationId,
                createdBy,
                request.RoleIds,
                _currentUserService.UserId.Value,
                request.ManagerId,
                request.ReportingOfficerId,
                cancellationToken);

            var error = (InviteUserError)errorCode;
            if (error != InviteUserError.None)
            {
                return InviteUserErrorMapper.ToFailure<Guid>(error);
            }

            Guid generatedUserId = newUserId ?? Guid.Empty;

            if (request.ManagerId.HasValue || request.ReportingOfficerId.HasValue)
            {
                // The new hire changes their manager's/reporting officer's (and every ancestor's)
                // hierarchy sets.
                await _userHierarchyService.InvalidateAsync();
            }

            await _welcomeEmailService.SendWelcomeEmailAsync(
                request.Email, request.FirstName, request.Username, request.Password, cancellationToken);

            return ApiResponse<Guid>.Success(generatedUserId, "User invited successfully.", 201);
        }
    }
}
