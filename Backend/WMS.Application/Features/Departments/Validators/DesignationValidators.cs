using FluentValidation;
using WMS.Application.Features.Departments.Commands;

namespace WMS.Application.Features.Departments.Validators
{
    public class CreateDesignationValidator : AbstractValidator<CreateDesignationCommand>
    {
        public CreateDesignationValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Department ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Designation name is required.")
                .MaximumLength(100).WithMessage("Designation name must not exceed 100 characters.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Designation code is required.")
                .MaximumLength(20).WithMessage("Designation code must not exceed 20 characters.")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Code can only contain letters, numbers, hyphens, and underscores.");

            RuleFor(x => x.Level)
                .GreaterThanOrEqualTo(1).WithMessage("Hierarchy level must be at least 1.");
        }
    }

    public class UpdateDesignationValidator : AbstractValidator<UpdateDesignationCommand>
    {
        public UpdateDesignationValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Designation ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Designation name is required.")
                .MaximumLength(100).WithMessage("Designation name must not exceed 100 characters.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Designation code is required.")
                .MaximumLength(20).WithMessage("Designation code must not exceed 20 characters.")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Code can only contain letters, numbers, hyphens, and underscores.");

            RuleFor(x => x.Level)
                .GreaterThanOrEqualTo(1).WithMessage("Hierarchy level must be at least 1.");
        }
    }
}
