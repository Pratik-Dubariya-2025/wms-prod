using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Auth.Commands
{
    public class LogoutCommand(string refreshToken) : IRequest<ApiResponse>
    {
        public string RefreshToken { get; } = refreshToken;
    }

    public class LogoutCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService) 
        : IRequestHandler<LogoutCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<ApiResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            string tokenHash = _tokenService.HashToken(request.RefreshToken);
            var storedToken = await _unitOfWork.RefreshToken.GetFirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                storedToken.ModifiedAt = DateTime.UtcNow;
                _unitOfWork.RefreshToken.Update(storedToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return ApiResponse.Success("Logged out successfully.");
        }
    }
}
