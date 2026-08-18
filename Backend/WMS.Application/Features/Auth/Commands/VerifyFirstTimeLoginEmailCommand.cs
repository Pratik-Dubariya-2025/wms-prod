using FluentValidation;
using MediatR;
using WMS.Application.Common.Models;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Application.Features.Auth.Commands
{
    public class VerifyFirstTimeLoginEmailCommand : IRequest<ApiResponse<string>>
    {
        public string Email { get; set; } = null!;
    }

    public class VerifyFirstTimeLoginEmailCommandValidator : AbstractValidator<VerifyFirstTimeLoginEmailCommand>
    {
        public VerifyFirstTimeLoginEmailCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");
        }
    }

    public class VerifyFirstTimeLoginEmailCommandHandler(IUnitOfWork unitOfWork) :
        IRequestHandler<VerifyFirstTimeLoginEmailCommand, ApiResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<string>> Handle(VerifyFirstTimeLoginEmailCommand request, CancellationToken cancellationToken)
        {
            User? user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted);
            if (user == null)
            {
                return ApiResponse<string>.Failure("Email is not registered.", statusCode: 400);
            }

            if (!user.IsFirstTimeLogin)
            {
                return ApiResponse<string>.Failure("This account has already completed first-time verification. Please login normally.", statusCode: 400);
            }

            return ApiResponse<string>.Success("Email verified successfully.", statusCode: 200);
        }
    }
}
