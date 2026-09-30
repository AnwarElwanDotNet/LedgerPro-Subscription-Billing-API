using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class PaymentAttemptDtoValidator : AbstractValidator<PaymentAttemptDto>
    {
        public PaymentAttemptDtoValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty()
                .WithMessage("Invoice identifier is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Payment attempt amount must be greater than zero.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid payment attempt status value.");

            RuleFor(x => x.ProviderReference)
                .MaximumLength(250)
                .WithMessage("Provider reference must not exceed 250 characters.")
                .When(x => !string.IsNullOrEmpty(x.ProviderReference));

            RuleFor(x => x.AttemptedAt)
                .NotEmpty()
                .WithMessage("Payment attempt date is required.");
        }
    }
}