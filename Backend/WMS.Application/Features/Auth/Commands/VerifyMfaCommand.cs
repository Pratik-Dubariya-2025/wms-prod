using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;

namespace WMS.Application.Features.Auth.Commands
{
    public class VerifyMfaCommand : IRequest<ApiResponse<bool>>
    {
        public string Code { get; set; } = null!;
    }

    public class VerifyMfaCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ITotpService totpService, IMfaSecretProtector mfaSecretProtector)
        : IRequestHandler<VerifyMfaCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly ITotpService _totpService = totpService;
        private readonly IMfaSecretProtector _mfaSecretProtector = mfaSecretProtector;

        public async Task<ApiResponse<bool>> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return ApiResponse<bool>.Failure("Unauthorized access.", null, 401);
            }

            Guid userId = _currentUserService.UserId.Value;
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
            {
                return ApiResponse<bool>.Failure("User not found.", null, 404);
            }

            if (string.IsNullOrEmpty(user.MfaSecret))
            {
                return ApiResponse<bool>.Failure("MFA setup has not been initialized for this user.", null, 400);
            }

            bool isValid = _totpService.VerifyCode(_mfaSecretProtector.Unprotect(user.MfaSecret), request.Code);

            if (!isValid)
            {
                return ApiResponse<bool>.Failure("Invalid authentication code.", null, 400);
            }

            user.IsMfaEnabled = true;
            user.ModifiedAt = DateTime.UtcNow;

            _unitOfWork.User.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "MFA enabled and verified successfully.");
        }
    }
}
