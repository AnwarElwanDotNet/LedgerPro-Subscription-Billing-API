using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("EmailRequired")
                .EmailAddress()
                .WithMessage("EmailInvalid")
                .MaximumLength(256)
                .WithMessage("EmailMaxLength");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("PasswordRequired")
                .MinimumLength(6)
                .WithMessage("PasswordLength")
                .MaximumLength(100)
                .WithMessage("PasswordLength");

            RuleFor(x => x.ReturnUrl)
                .MaximumLength(500)
                .WithMessage("ReturnUrlMaxLength");
        }
    }
}