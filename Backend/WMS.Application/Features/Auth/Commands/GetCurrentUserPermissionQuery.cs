using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Auth.Commands
{
    public class GetCurrentUserPermissionQuery : IRequest<ApiResponse<List<string>>>
    {
    }

    public class GetCurrentUserPermissionQueryHandler
        (ICurrentUserService currentUserService, IPermissionCacheService permissionCacheService) : IRequestHandler<GetCurrentUserPermissionQuery, ApiResponse<List<string>>>
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPermissionCacheService _permissionCacheService = permissionCacheService;

        public async Task<ApiResponse<List<string>>> Handle(GetCurrentUserPermissionQuery request, CancellationToken cancellationToken)
        {
            Guid? userId = _currentUserService.UserId;

            if (userId == null)
            {
                return ApiResponse<List<string>>.Failure("User is not authenticated.", statusCode: 401);
            }

            List<string> permissions = await _permissionCacheService.GetPermissionsAsync(userId.Value);

            return ApiResponse<List<string>>.Success(permissions);
        }
    }
}
