using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class RefundDtoValidator : AbstractValidator<RefundDto>
    {
        public RefundDtoValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty()
                .WithMessage("Payment identifier is required.");

            RuleFor(x => x.RefundAmount)
                .GreaterThan(0)
                .WithMessage("Refund amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Length(3)
                .WithMessage("Currency must be exactly 3 characters.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid refund status value.");

            RuleFor(x => x.RefundDate)
                .NotEmpty()
                .WithMessage("Refund date is required.");
        }
    }
}