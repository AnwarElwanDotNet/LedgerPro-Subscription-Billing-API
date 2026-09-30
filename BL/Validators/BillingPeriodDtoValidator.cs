using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class BillingPeriodDtoValidator : AbstractValidator<BillingPeriodDto>
    {
        public BillingPeriodDtoValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription identifier is required.");

            RuleFor(x => x.PlanId)
                .NotEmpty()
                .WithMessage("Plan identifier is required.");

            RuleFor(x => x.Start)
                .NotEmpty()
                .WithMessage("Billing period start date is required.");

            RuleFor(x => x.End)
                .NotEmpty()
                .WithMessage("Billing period end date is required.")
                .GreaterThan(x => x.Start)
                .WithMessage("Billing period end date must be later than the start date.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Billing period amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Length(3)
                .WithMessage("Currency must be exactly 3 characters.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid billing period status value.");
        }
    }
}