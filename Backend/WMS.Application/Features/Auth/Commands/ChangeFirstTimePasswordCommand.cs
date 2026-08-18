using FluentValidation;
using MediatR;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Auth.Commands
{
    public class ChangeFirstTimePasswordCommand : IRequest<ApiResponse<string>>
    {
        public string Email { get; set; } = null!;
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class ChangeFirstTimePasswordCommandValidator : AbstractValidator<ChangeFirstTimePasswordCommand>
    {
        public ChangeFirstTimePasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        }
    }


    public class ChangeFirstTimePasswordCommandHandler(IUnitOfWork unitOfWork) :
        IRequestHandler<ChangeFirstTimePasswordCommand, ApiResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<string>> Handle(ChangeFirstTimePasswordCommand request, CancellationToken cancellationToken)
        {
            User? user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted);
            if (user == null)
            {
                return ApiResponse<string>.Failure("User not found.", statusCode: 400);
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return ApiResponse<string>.Failure("Current password is wrong", statusCode: 400);
            }

            if (!user.IsFirstTimeLogin)
            {
                return ApiResponse<string>.Failure("Password has already been changed. Please log in normally.", statusCode: 400);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.IsFirstTimeLogin = false;
            _unitOfWork.User.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<string>.Success("Password changed successfully. You can now log in.", statusCode: 200);
        }
    }
}
