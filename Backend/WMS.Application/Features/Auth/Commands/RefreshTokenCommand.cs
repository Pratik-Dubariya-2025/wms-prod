using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using WMS.Application.Common.Exceptions;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Auth.DTOs;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Auth.Commands
{
    public class RefreshTokenCommand : IRequest<ApiResponse<AuthResponseDto>>
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class RefreshTokenCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<ApiResponse<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Extract ClaimsPrincipal from the expired access token
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                return ApiResponse<AuthResponseDto>.Failure("Invalid access token or claims.", null, 400);
            }

            // 2. Retrieve user ID from sub claim
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return ApiResponse<AuthResponseDto>.Failure("Invalid user claim.", null, 400);
            }

            // 3. Hash the incoming refresh token to match database record
            string incomingTokenHash = HashToken(request.RefreshToken);

            // 4. Retrieve the active refresh token record from database along with all user details
            var storedToken = await _unitOfWork.RefreshToken.GetActiveTokenWithUserAndRolesAsync(userId, incomingTokenHash, cancellationToken);

            if (storedToken == null || !storedToken.IsActive)
            {
                return ApiResponse<AuthResponseDto>.Failure("Invalid, expired, or revoked refresh token.", null, 401);
            }

            var user = storedToken.User;
            if (user == null || !user.IsActive)
            {
                throw new NotFoundException(nameof(User), userId);
            }

            // 5. Revoke the old refresh token
            storedToken.IsRevoked = true;
            _unitOfWork.RefreshToken.Update(storedToken);

            // 6. Generate new Access and Refresh tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshTokenString = _tokenService.GenerateRefreshToken();
            var newRefreshTokenHash = HashToken(newRefreshTokenString);

            // Check if rememberMe was set by checking original lifespan (7 days vs 1 day)
            var originalLifespan = storedToken.ExpiresAt - storedToken.CreatedAt;
            bool rememberMe = originalLifespan.TotalDays > 2;
            var refreshTokenLifespan = rememberMe ? TimeSpan.FromDays(7) : TimeSpan.FromDays(1);
            var refreshTokenExpiry = DateTime.UtcNow.Add(refreshTokenLifespan);

            // 7. Save new Refresh Token
            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = refreshTokenExpiry,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshToken.AddAsync(newRefreshToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var responseDto = new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                Expiration = newRefreshToken.ExpiresAt
            };

            return ApiResponse<AuthResponseDto>.Success(responseDto, "Token refreshed successfully.", 200);
        }

        private static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
