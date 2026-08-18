using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Users.Commands
{
    [RequirePermission(PermissionCodes.UserUpdate)]
    public class UpdateUserCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid DesignationId { get; set; }
        public bool IsActive { get; set; }
        public List<Guid> RoleIds { get; set; } = [];
    }

    public class UpdateUserCommandHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        IPermissionCacheService permissionCacheService) 
        : IRequestHandler<UpdateUserCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPermissionCacheService _permissionCacheService = permissionCacheService;

        public async Task<ApiResponse<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            Guid currentUserId = _currentUserService.UserId.Value;

            var user = await _unitOfWork.User.IncludeAndGetFirstOrDefaultAsync<object>(
                u => u.Id == request.Id && !u.IsDeleted,
                u => u.Designation,
                u => u.UserRoles
            );

            if (user == null)
            {
                return ApiResponse<bool>.Failure("User not found.", null, 404);
            }

            var currentUser = await _unitOfWork.User.IncludeAndGetFirstOrDefaultAsync<object>(
                u => u.Id == currentUserId && !u.IsDeleted,
                u => u.Designation,
                u => u.UserRoles
            );

            if (currentUser == null)
            {
                return ApiResponse<bool>.Failure("Current user details not found.", null, 401);
            }

            var dept = await _unitOfWork.Department.GetFirstOrDefaultAsync(d => d.Id == request.DepartmentId && !d.IsDeleted);
            if (dept == null)
            {
                return ApiResponse<bool>.Failure("Department not found.", null, 400);
            }

            var desig = await _unitOfWork.Designation.GetFirstOrDefaultAsync(d => d.Id == request.DesignationId && !d.IsDeleted);
            if (desig == null)
            {
                return ApiResponse<bool>.Failure("Designation not found.", null, 400);
            }

            if (user.Id != currentUserId)
            {
                if (user.Designation.Level >= currentUser.Designation.Level)
                {
                    return ApiResponse<bool>.Failure("Hierarchy violation: You cannot modify a user with a rank equal to or higher than yours.", null, 403);
                }

                if (desig.Level >= currentUser.Designation.Level)
                {
                    return ApiResponse<bool>.Failure("Hierarchy violation: You cannot assign a designation rank equal to or higher than yours.", null, 403);
                }

                var requestedRoles = await _unitOfWork.Role.GetAllAsync(r => r, r => request.RoleIds.Contains(r.Id) && !r.IsDeleted);
                int requestedMaxRoleRank = GetMaxRoleRank(requestedRoles.Select(r => r.Name));
                
                var currentUserRoles = await _unitOfWork.Role.GetAllAsync(r => r, r => currentUser.UserRoles.Select(ur => ur.RoleId).Contains(r.Id));
                int currentUserMaxRoleRank = GetMaxRoleRank(currentUserRoles.Select(r => r.Name));

                if (requestedMaxRoleRank >= currentUserMaxRoleRank)
                {
                    return ApiResponse<bool>.Failure("Hierarchy violation: You cannot assign roles with a rank equal to or higher than yours.", null, 403);
                }
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.DepartmentId = request.DepartmentId;
            user.DesignationId = request.DesignationId;
            user.IsActive = request.IsActive;
            user.ModifiedBy = _currentUserService.Username ?? "System";
            user.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.User.Update(user);

            var existingUserRoles = await _unitOfWork.UserRole.GetAllAsync(ur => ur, ur => ur.UserId == user.Id);
            foreach (var ur in existingUserRoles)
            {
                _unitOfWork.UserRole.Remove(ur);
            }

            foreach (var roleId in request.RoleIds)
            {
                var newUserRole = new UserRole { UserId = user.Id, RoleId = roleId };
                await _unitOfWork.UserRole.AddAsync(newUserRole);
            }

            // Handle automatic team creation/dissociation for Tech Leads
            if (desig.Name == "Tech Lead")
            {
                var existingTeam = await _unitOfWork.Team.GetFirstOrDefaultAsync(t => t.TeamLeadId == user.Id && !t.IsDeleted);
                if (existingTeam == null)
                {
                    var team = new Team
                    {
                        Name = $"{request.FirstName} {request.LastName}'s Team",
                        DepartmentId = request.DepartmentId,
                        TeamLeadId = user.Id,
                        CreatedBy = _currentUserService.Username ?? "System"
                    };
                    await _unitOfWork.Team.AddAsync(team);
                    user.TeamId = team.Id; // one Tech Lead ⇒ one team
                }
                else
                {
                    existingTeam.DepartmentId = request.DepartmentId;
                    existingTeam.Name = $"{request.FirstName} {request.LastName}'s Team";
                    existingTeam.ModifiedBy = _currentUserService.Username ?? "System";
                    existingTeam.ModifiedAt = DateTime.UtcNow;
                    _unitOfWork.Team.Update(existingTeam);
                    user.TeamId = existingTeam.Id;
                }
            }
            else
            {
                // Demoted out of Tech Lead — detach from any team they led.
                user.TeamId = null;
                var existingTeams = await _unitOfWork.Team.GetAllAsync(t => t, t => t.TeamLeadId == user.Id && !t.IsDeleted);
                foreach (var t in existingTeams)
                {
                    t.TeamLeadId = null;
                    t.ModifiedBy = _currentUserService.Username ?? "System";
                    t.ModifiedAt = DateTime.UtcNow;
                    _unitOfWork.Team.Update(t);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _permissionCacheService.InvalidateUser(user.Id);

            return ApiResponse<bool>.Success(true, "User updated successfully.");
        }

        private static int GetMaxRoleRank(IEnumerable<string> roleNames)
        {
            int max = 0;
            foreach (var name in roleNames)
            {
                int rank = name.ToUpperInvariant() switch
                {
                    "ADMIN" => 4,
                    "HR_MANAGER" => 3,
                    "TEAM_LEAD" => 2,
                    "EMPLOYEE" => 1,
                    _ => 0
                };
                if (rank > max) max = rank;
            }
            return max;
        }
    }
}
