using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Auth.DTOs;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Auth.Commands
{
    public class SetupMfaCommand : IRequest<ApiResponse<MfaSetupDto>>
    {
    }

    public class SetupMfaCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ITotpService totpService, IMfaSecretProtector mfaSecretProtector)
        : IRequestHandler<SetupMfaCommand, ApiResponse<MfaSetupDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly ITotpService _totpService = totpService;
        private readonly IMfaSecretProtector _mfaSecretProtector = mfaSecretProtector;

        public async Task<ApiResponse<MfaSetupDto>> Handle(SetupMfaCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<MfaSetupDto>.Failure("Unauthorized access.", null, 401);
            }

            Guid userId = _currentUserService.UserId.Value;
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
            {
                return ApiResponse<MfaSetupDto>.Failure("User not found.", null, 404);
            }

            // Generate TOTP secret and QR URL
            string secret = _totpService.GenerateSecret();
            string qrCodeUrl = _totpService.GetQrCodeUrl(user.Email, secret);

            // Store the secret encrypted at rest; the raw value below is only
            // ever returned to the owning user for their authenticator app to
            // scan/enter, never persisted in plaintext. Keep MFA disabled until verified.
            user.MfaSecret = _mfaSecretProtector.Protect(secret);
            user.IsMfaEnabled = false;
            user.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.User.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new MfaSetupDto
            {
                SharedSecret = secret,
                QrCodeUrl = qrCodeUrl
            };

            return ApiResponse<MfaSetupDto>.Success(dto, "MFA secret generated successfully. Please verify a code to complete setup.");
        }
    }
}
