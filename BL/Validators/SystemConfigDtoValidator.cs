using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class SystemConfigDtoValidator : AbstractValidator<SystemConfigDto>
    {
        public SystemConfigDtoValidator()
        {
            RuleFor(x => x.TrialDurationDays)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Trial duration cannot be negative.");

            RuleFor(x => x.GracePeriodDays)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Grace period duration cannot be negative.");

            RuleFor(x => x.RetryDay1)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Retry day 1 cannot be negative.");

            RuleFor(x => x.RetryDay2)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Retry day 2 cannot be negative.");

            RuleFor(x => x.RetryDay3)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Retry day 3 cannot be negative.");

            RuleFor(x => x.RetryDay2)
                .GreaterThan(x => x.RetryDay1)
                .WithMessage("Retry day 2 must be later than retry day 1.");

            RuleFor(x => x.RetryDay3)
                .GreaterThan(x => x.RetryDay2)
                .WithMessage("Retry day 3 must be later than retry day 2.");
        }
    }
}