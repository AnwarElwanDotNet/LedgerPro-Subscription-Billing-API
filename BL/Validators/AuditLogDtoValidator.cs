using BL.Dtos;
using Domain.Enums;
using FluentValidation;

namespace BL.Validators
{
    public class AuditLogDtoValidator : AbstractValidator<AuditLogDto>
    {
        public AuditLogDtoValidator()
        {
            RuleFor(x => x.Action)
                .NotEmpty()
                .WithMessage("Audit action is required.")
                .MaximumLength(250)
                .WithMessage("Audit action must not exceed 250 characters.");

            RuleFor(x => x.EntityType)
                .IsInEnum()
                .WithMessage("Invalid entity type value.");

            RuleFor(x => x.EntityId)
                .NotEmpty()
                .WithMessage("Entity identifier is required.");

            RuleFor(x => x.Metadata)
                .MaximumLength(4000)
                .WithMessage("Metadata must not exceed 4000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Metadata));

            RuleFor(x => x.CorrelationId)
                .NotEqual(Guid.Empty)
                .WithMessage("Correlation identifier cannot be empty.")
                .When(x => x.CorrelationId.HasValue);

         
        }
    }
}