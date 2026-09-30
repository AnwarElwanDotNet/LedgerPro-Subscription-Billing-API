using BL.Dtos;
using Domain.Enums;
using FluentValidation;

namespace BL.Validators
{
    public class CancellationRecordDtoValidator : AbstractValidator<CancellationRecordDto>
    {
        public CancellationRecordDtoValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization identifier is required.");

            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription identifier is required.");

            RuleFor(x => x.PerformedBy)
                .NotEmpty()
                .WithMessage("Performed by user identifier is required.");

            RuleFor(x => x.PerformedAt)
                .NotEmpty()
                .WithMessage("Cancellation performed date is required.");

            RuleFor(x => x.EffectiveDate)
                .NotEmpty()
                .WithMessage("Cancellation effective date is required.")
                .GreaterThanOrEqualTo(x => x.PerformedAt)
                .WithMessage("Cancellation effective date cannot be before the cancellation performed date.");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Cancellation reason must not exceed 500 characters.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid cancellation type value.");

            When(x => x.Type == CancellationType.Immediate, () =>
            {
                RuleFor(x => x.AccessEndedImmediately)
                    .Equal(true)
                    .WithMessage("Access must be ended immediately for an immediate cancellation.");
            });

            When(x => x.Type == CancellationType.EndOfPeriod, () =>
            {
                RuleFor(x => x.AccessEndedImmediately)
                    .Equal(false)
                    .WithMessage("Access must not end immediately for a period-end cancellation.");
            });
        }
    }
}