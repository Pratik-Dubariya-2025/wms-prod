using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Auth.Commands
{
    public class LogoutAllCommand : IRequest<ApiResponse>
    {
    }

    public class LogoutAllCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) 
        : IRequestHandler<LogoutAllCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse.Failure("Unauthorized access.", null, 401);
            }

            Guid userId = _currentUserService.UserId.Value;

            var activeTokens = await _unitOfWork.RefreshToken.GetAllAsync(t => t.UserId == userId && !t.IsRevoked);

            if (activeTokens.Count != 0)
            {
                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                    token.ModifiedAt = DateTime.UtcNow;
                    _unitOfWork.RefreshToken.Update(token);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return ApiResponse.Success("All active sessions revoked successfully.");
        }
    }
}
