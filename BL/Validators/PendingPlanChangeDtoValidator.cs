using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class PendingPlanChangeDtoValidator : AbstractValidator<PendingPlanChangeDto>
    {
        public PendingPlanChangeDtoValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription identifier is required.");

            RuleFor(x => x.TargetPlanId)
                .NotEmpty()
                .WithMessage("Target plan identifier is required.");

            RuleFor(x => x.EffectiveDate)
                .NotEmpty()
                .WithMessage("Effective date is required.");

            RuleFor(x => x.EffectiveDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Effective date must be in the future.");

         
        }
    }
}