using FluentValidation;
using WMS.Application.Features.Departments.Commands;

namespace WMS.Application.Features.Departments.Validators
{
    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Department ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Department name is required.")
                .MaximumLength(150).WithMessage("Department name must not exceed 150 characters.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Department code is required.")
                .MaximumLength(50).WithMessage("Department code must not exceed 50 characters.")
                .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Department code can only contain letters, numbers, hyphens, and underscores.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
