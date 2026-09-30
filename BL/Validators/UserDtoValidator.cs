using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email address format.")
                .MaximumLength(150)
                .WithMessage("Email must not exceed 150 characters.");

            RuleFor(x => x.Password)
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(100)
                .WithMessage("Password must not exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Password));

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match.")
                .When(x => !string.IsNullOrEmpty(x.ConfirmPassword));

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .MaximumLength(50)
                .WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(50)
                .WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .MaximumLength(20)
                .WithMessage("Phone number must not exceed 20 characters.")
                .Matches(@"^\+?[0-9\s\-\(\)]+$")
                .WithMessage("Invalid phone number format.");

            RuleFor(x => x.Role)
                .MaximumLength(50)
                .WithMessage("Role name must not exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.Role));

            RuleFor(x => x.ReturnUrl)
                .MaximumLength(500)
                .WithMessage("Return URL must not exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.ReturnUrl));

            RuleForEach(x => x.Roles)
                .NotEmpty()
                .WithMessage("Role name is required.")
                .MaximumLength(50)
                .WithMessage("Role name must not exceed 50 characters.");
        }
    }
}