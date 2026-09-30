using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class GracePeriodDtoValidator : AbstractValidator<GracePeriodDto>
    {
        public GracePeriodDtoValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription identifier is required.");

            RuleFor(x => x.InvoiceId)
                .NotEmpty()
                .WithMessage("Invoice identifier is required.");

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("Grace period start date is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("Grace period end date is required.")
                .GreaterThan(x => x.StartDate)
                .WithMessage("Grace period end date must be later than the start date.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid grace period status value.");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Grace period reason must not exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Reason));
        }
    }
}