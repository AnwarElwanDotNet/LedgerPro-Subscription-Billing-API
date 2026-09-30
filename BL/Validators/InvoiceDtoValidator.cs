using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class InvoiceDtoValidator : AbstractValidator<InvoiceDto>
    {
        public InvoiceDtoValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization identifier is required.");

            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription identifier is required.");

            RuleFor(x => x.PlanId)
                .NotEmpty()
                .WithMessage("Plan identifier is required.");

            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .WithMessage("Invoice duration must be greater than zero.");

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("Invoice start date is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("Invoice end date is required.")
                .GreaterThan(x => x.StartDate)
                .WithMessage("Invoice end date must be later than the start date.");

            RuleFor(x => x.PlanNameSnapshot)
                .NotEmpty()
                .WithMessage("Plan name snapshot is required.")
                .MaximumLength(200)
                .WithMessage("Plan name snapshot must not exceed 200 characters.");

            RuleFor(x => x.PlanUserLimitSnapshot)
                .GreaterThan(0)
                .WithMessage("Plan user limit snapshot must be greater than zero.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Invoice amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .Length(3)
                .WithMessage("Currency must be exactly 3 characters.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid invoice status value.");

            RuleFor(x => x.InvoiceType)
                .IsInEnum()
                .WithMessage("Invalid invoice type value.");
        }
    }
}