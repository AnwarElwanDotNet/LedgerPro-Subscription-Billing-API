using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class SubscriptionDtoValidator : AbstractValidator<SubscriptionDto>
    {
        public SubscriptionDtoValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization identifier is required.");

            RuleFor(x => x.PlanId)
                .NotEmpty()
                .WithMessage("Plan identifier is required.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid subscription status value.");

            RuleFor(x => x.CurrentPeriodEnd)
                .GreaterThan(x => x.CurrentPeriodStart)
                .When(x => x.CurrentPeriodStart.HasValue && x.CurrentPeriodEnd.HasValue)
                .WithMessage("Current period end date must be later than the start date.");

            RuleFor(x => x.StartedAt)
                .LessThanOrEqualTo(x => x.CurrentPeriodStart)
                .When(x => x.StartedAt.HasValue && x.CurrentPeriodStart.HasValue)
                .WithMessage("Subscription start date cannot be later than the current period start date.");
        }
    }
}