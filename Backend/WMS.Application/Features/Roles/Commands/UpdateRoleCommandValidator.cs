using FluentValidation;

namespace WMS.Application.Features.Roles.Commands
{
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Priority).InclusiveBetween(1, 1000)
                .WithMessage("Priority must be between 1 and 1000.");
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}
