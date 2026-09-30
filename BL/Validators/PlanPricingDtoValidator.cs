using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class PlanPricingDtoValidator : AbstractValidator<PlanPricingDto>
    {
        public PlanPricingDtoValidator()
        {
            RuleFor(x => x.PlanId)
                .NotEmpty()
                .WithMessage("Plan identifier is required.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Plan price must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Length(3)
                .WithMessage("Currency must be exactly 3 characters.");

            RuleFor(x => x.EffectiveDate)
                .NotEmpty()
                .WithMessage("Pricing effective date is required.");
        }
    }
}