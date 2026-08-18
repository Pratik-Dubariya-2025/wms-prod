using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Auth.Commands
{
    public class ResetPasswordCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService) 
        : IRequestHandler<ResetPasswordCommand, ApiResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<ApiResponse<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.CookieToken) || request.Token != request.CookieToken)
            {
                return ApiResponse<string>.Failure("Invalid reset session or cookie mismatch.", statusCode: 400);
            }

            string hashedToken = _tokenService.HashToken(request.Token);

            ForgotPassword? forgotPassword = await _unitOfWork.ForgotPassword.IncludeAndGetFirstOrDefaultAsync<object>(
                fp => fp.TokenHash == hashedToken && !fp.IsDeleted && fp.ExpireAt > DateTime.UtcNow,
                fp => fp.User
            );

            if (forgotPassword == null)
            {
                return ApiResponse<string>.Failure("Invalid or expired reset token.", statusCode: 400);
            }

            User user = forgotPassword.User;
            if (user == null || user.IsDeleted)
            {
                return ApiResponse<string>.Failure("User not found.", statusCode: 404);
            }

            // Update user password and clear lockout status
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;

            _unitOfWork.User.Update(user);

            // Invalidate/Soft-delete the reset token to prevent reuse
            forgotPassword.IsDeleted = true;
            forgotPassword.DeletedAt = DateTime.UtcNow;
            forgotPassword.DeletedBy = user.Username;
            _unitOfWork.ForgotPassword.Update(forgotPassword);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<string>.Success("Password reset successfully.", null, 200);
        }
    }
}
