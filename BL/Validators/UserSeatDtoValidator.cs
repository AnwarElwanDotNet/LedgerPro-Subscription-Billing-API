using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class UserSeatDtoValidator : AbstractValidator<UserSeatDto>
    {
        public UserSeatDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User identifier is required.");

            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("Organization identifier is required.");

            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription identifier is required.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid user seat status value.");
        }
    }
}