using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email address format.")
                .MaximumLength(150)
                .WithMessage("Email cannot exceed 150 characters.");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Token is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required.")
                .MinimumLength(8)
                .WithMessage("New password must be at least 8 characters.")
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