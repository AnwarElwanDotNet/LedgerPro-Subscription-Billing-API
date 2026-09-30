using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class PlanDtoValidator : AbstractValidator<PlanDto>
    {
        public PlanDtoValidator()
        {
            RuleFor(x => x.PlanName)
                .NotEmpty()
                .WithMessage("Plan name is required.")
                .MaximumLength(200)
                .WithMessage("Plan name must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Plan description must not exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.CurrentPrice)
                .GreaterThan(0)
                .WithMessage("Plan price must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Length(3)
                .WithMessage("Currency must be exactly 3 characters.");

            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .WithMessage("Plan duration must be greater than zero.");

            RuleFor(x => x.MaxUsers)
                .GreaterThan(0)
                .WithMessage("Maximum users must be greater than zero.");
        }
    }
}