using FluentValidation;

namespace WMS.Application.Features.Policies.Commands
{
    public class UpdateAccessPolicyCommandValidator : AbstractValidator<UpdateAccessPolicyCommand>
    {
        public UpdateAccessPolicyCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.ModuleId).NotEmpty();
            RuleFor(x => x.Action).NotEmpty();
            RuleFor(x => x.Effect).Must(e => e == "Allow" || e == "Deny")
                .WithMessage("Effect must be 'Allow' or 'Deny'.");
            RuleFor(x => x.Priority).InclusiveBetween(1, 1000)
                .WithMessage("Priority must be between 1 and 1000.");
            RuleFor(x => x)
                .Must(x => x.RoleId.HasValue ^ x.UserId.HasValue)
                .WithMessage("Exactly one of RoleId or UserId must be specified.");
        }
    }
}
