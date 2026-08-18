using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Auth.Commands
{
    public class ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IEmailService emailService) 
        : IRequestHandler<ForgotPasswordCommand, ApiResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IEmailService _emailService = emailService;

        public async Task<ApiResponse<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            User? user = await _unitOfWork.User.IncludeAndGetFirstOrDefaultAsync<object>(
                s => s.Email.ToLower() == request.Email.ToLower(),
                s => s.ForgotPasswords
            );

            if (user == null)
            {
                return ApiResponse<string>.Failure("Email not found", statusCode: 404);
            }

            if (user.ForgotPasswords != null && user.ForgotPasswords.Any(fp => fp.CreatedAt > DateTime.UtcNow.AddMinutes(-30)))
            {
                return ApiResponse<string>.Failure("A password reset request has already been made within the last 30 minutes. Please check your email or try again later.", statusCode: 400);
            }

            string resetToken = _tokenService.GenerateRefreshToken();
            string hashedToken = _tokenService.HashToken(resetToken);

            ForgotPassword forgotPassword = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = hashedToken,
                ExpireAt = DateTime.UtcNow.AddMinutes(30)
            };

            await _unitOfWork.ForgotPassword.AddAsync(forgotPassword);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string resetLink = $"http://localhost:5173/reset-password?token={resetToken}";
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "WMS.Application.Common.Templates.ResetPasswordTemplate.html";
            string templateContent = string.Empty;

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' was not found.");
                }
                using (StreamReader reader = new(stream))
                {
                    templateContent = await reader.ReadToEndAsync(cancellationToken);
                }
            }

            string emailBody = string.Format(templateContent, user.FirstName, resetLink);

            await _emailService.SendEmailAsync(user.Email, "Reset Your WMS Password", emailBody);

            return ApiResponse<string>.Success($"An email with reset password link has been sent to your {user.Email} address!", resetToken, 200);
        }
    }
}
