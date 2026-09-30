using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class NotificationDtoValidator : AbstractValidator<NotificationDto>
    {
        public NotificationDtoValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization identifier is required.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Notification description is required.")
                .MaximumLength(1000)
                .WithMessage("Notification description must not exceed 1000 characters.");

            RuleFor(x => x.RelatedEntityType)
                .IsInEnum()
                .WithMessage("Invalid related entity type value.");

            RuleFor(x => x.RelatedEntityId)
                .NotEmpty()
                .WithMessage("Related entity identifier is required.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid notification type value.");
        }
    }
}