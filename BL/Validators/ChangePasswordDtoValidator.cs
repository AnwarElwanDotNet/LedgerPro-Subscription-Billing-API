using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required.")
                .MinimumLength(6)
                .WithMessage("New password must be at least 6 characters.")
                .MaximumLength(100)
                .WithMessage("New password must not exceed 100 characters.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required.")
                .Equal(x => x.NewPassword)
                .WithMessage("The new password and confirmation password do not match.");
        }
    }
}