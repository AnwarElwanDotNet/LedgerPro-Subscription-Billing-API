using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class EditProfileDtoValidator : AbstractValidator<EditProfileDto>
    {
        public EditProfileDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("FirstNameRequired")
                .MaximumLength(50)
                .WithMessage("FirstNameMaxLength");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("LastNameRequired")
                .MaximumLength(50)
                .WithMessage("LastNameMaxLength");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("PhoneRequired")
                .Matches(@"^\+?[0-9\s\-\(\)]+$")
                .WithMessage("PhoneInvalid");
        }
    }
}