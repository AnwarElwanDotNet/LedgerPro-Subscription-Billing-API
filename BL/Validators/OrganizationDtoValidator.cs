using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
    public class OrganizationDtoValidator : AbstractValidator<OrganizationDto>
    {
        public OrganizationDtoValidator()
        {
            RuleFor(x => x.OrganizationName)
                .NotEmpty()
                .WithMessage("Organization name is required.")
                .MaximumLength(200)
                .WithMessage("Organization name must not exceed 200 characters.");
        }
    }
}