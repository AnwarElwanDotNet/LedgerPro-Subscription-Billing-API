using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class TrialDtoValidator : AbstractValidator<TrialDto>
    {
        public TrialDtoValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization identifier is required.");

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("Trial start date is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("Trial end date is required.")
                .GreaterThan(x => x.StartDate)
                .WithMessage("Trial end date must be later than the start date.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid trial status value.");
        }
    }
}