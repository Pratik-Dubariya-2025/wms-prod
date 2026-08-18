using FluentValidation;

namespace WMS.Application.Features.Roles.Commands
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Priority).InclusiveBetween(1, 1000)
                .WithMessage("Priority must be between 1 and 1000.");
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}
