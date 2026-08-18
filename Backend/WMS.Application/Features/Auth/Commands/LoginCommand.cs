using FluentValidation;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Auth.DTOs;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Auth.Commands
{
    public class LoginCommand : IRequest<ApiResponse<AuthResponseDto>>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool? RememberMe { get; set; } = false;
        public string? MfaCode { get; set; }
    }

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }

    public class LoginCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService, ITotpService totpService, IMfaSecretProtector mfaSecretProtector) :
        IRequestHandler<LoginCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITokenService _tokenService = tokenService;
        private readonly ITotpService _totpService = totpService;
        private readonly IMfaSecretProtector _mfaSecretProtector = mfaSecretProtector;

        public async Task<ApiResponse<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            User? user = await _unitOfWork.User.IncludeAndGetFirstOrDefaultAsync<object>(
                s => s.Email.ToLower() == request.Email.ToLower(),
                s => s.Department,
                s => s.Designation
            );

            if (user == null)
            {
                return ApiResponse<AuthResponseDto>.Failure("Email or Password is wrong", statusCode: 400);
            }

            // Lockout check
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var minutesLeft = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                return ApiResponse<AuthResponseDto>.Failure($"Account is locked. Try again in {minutesLeft} minutes.", statusCode: 400);
            }

            if (!Verify(request.Password, user.PasswordHash))
            {
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                }
                _unitOfWork.User.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<AuthResponseDto>.Failure("Email or Password is wrong", statusCode: 400);
            }

            // Reset failed count / lockout on success
            if (user.AccessFailedCount > 0 || user.LockoutEnd != null)
            {
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                _unitOfWork.User.Update(user);
            }

            if (user.IsFirstTimeLogin)
            {
                return ApiResponse<AuthResponseDto>.Failure("FIRST_TIME_LOGIN", statusCode: 400);
            }

            if (user.IsMfaEnabled)
            {
                if (string.IsNullOrEmpty(request.MfaCode))
                {
                    return ApiResponse<AuthResponseDto>.Success(new AuthResponseDto
                    {
                        RequiresMfa = true
                    }, "MFA verification required.", 200);
                }

                if (!_totpService.VerifyCode(_mfaSecretProtector.Unprotect(user.MfaSecret!), request.MfaCode))
                {
                    return ApiResponse<AuthResponseDto>.Failure("Invalid MFA code.", null, 400);
                }
            }



            bool rememberMe = request.RememberMe ?? false;
            var refreshTokenLifespan = rememberMe ? TimeSpan.FromDays(7) : TimeSpan.FromDays(1);
            var refreshTokenExpiry = DateTime.UtcNow.Add(refreshTokenLifespan);

            string accessToken = _tokenService.GenerateAccessToken(user);
            string refreshTokenString = _tokenService.GenerateRefreshToken();
            string refreshTokenHash = _tokenService.HashToken(refreshTokenString);

            RefreshToken refreshToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                ExpiresAt = refreshTokenExpiry,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshToken.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            AuthResponseDto authResponse = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                Expiration = refreshToken.ExpiresAt
            };
            return ApiResponse<AuthResponseDto>.Success(authResponse, "Logged in successfully.", 200);
        }

        private static bool Verify(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
    }
}
