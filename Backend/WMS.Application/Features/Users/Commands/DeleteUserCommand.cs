using MediatR;
using WMS.Application.Common.Attributes;
using WMS.Application.Common.Constants;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Users.Commands
{
    [RequirePermission(PermissionCodes.UserDelete)]
    public class DeleteUserCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteUserCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) 
        : IRequestHandler<DeleteUserCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            Guid currentUserId = _currentUserService.UserId.Value;

            if (request.Id == currentUserId)
            {
                return ApiResponse<bool>.Failure("You cannot delete your own account.", null, 400);
            }

            var user = await _unitOfWork.User.IncludeAndGetFirstOrDefaultAsync<object>(
                u => u.Id == request.Id && !u.IsDeleted,
                u => u.Designation
            );

            if (user == null)
            {
                return ApiResponse<bool>.Failure("User not found.", null, 404);
            }

            var currentUser = await _unitOfWork.User.IncludeAndGetFirstOrDefaultAsync<object>(
                u => u.Id == currentUserId && !u.IsDeleted,
                u => u.Designation
            );

            if (currentUser == null)
            {
                return ApiResponse<bool>.Failure("Current user details not found.", null, 401);
            }

            // Designation level check
            if (user.Designation.Level >= currentUser.Designation.Level)
            {
                return ApiResponse<bool>.Failure("Hierarchy violation: You cannot delete a user with a rank equal to or higher than yours.", null, 403);
            }

            // Perform soft delete
            user.IsDeleted = true;
            user.IsActive = false;
            user.DeletedBy = _currentUserService.Username ?? "System";
            user.DeletedAt = DateTime.UtcNow;

            _unitOfWork.User.Update(user);

            // Revoke active refresh tokens
            var activeTokens = await _unitOfWork.RefreshToken.GetAllAsync(t => t.UserId == user.Id && !t.IsRevoked);
            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.ModifiedAt = DateTime.UtcNow;
                _unitOfWork.RefreshToken.Update(token);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "User deleted successfully.");
        }
    }
}
