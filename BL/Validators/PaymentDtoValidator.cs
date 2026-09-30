using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class PaymentDtoValidator : AbstractValidator<PaymentDto>
    {
        public PaymentDtoValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty()
                .WithMessage("Invoice identifier is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Payment amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Length(3)
                .WithMessage("Currency must be exactly 3 characters.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid payment status value.");

            RuleFor(x => x.ProviderReference)
                .MaximumLength(250)
                .WithMessage("Provider reference must not exceed 250 characters.")
                .When(x => !string.IsNullOrEmpty(x.ProviderReference));

            RuleFor(x => x.PaymentDate)
                .NotEmpty()
                .WithMessage("Payment date is required.");
        }
    }
}