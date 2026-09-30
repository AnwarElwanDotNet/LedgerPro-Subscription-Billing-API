using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class ChangeEmailDtoValidator : AbstractValidator<ChangeEmailDto>
    {
        public ChangeEmailDtoValidator()
        {
            RuleFor(x => x.NewEmail)
                .NotEmpty()
                .WithMessage("New email is required.")
                .EmailAddress()
                .WithMessage("Invalid email format.")
                .MaximumLength(256)
                .WithMessage("Email cannot exceed 256 characters.");
        }
    }
}