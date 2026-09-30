using BL.Dtos;
using FluentValidation;

namespace BL.Validators
{
	public class UserResultDtoValidator : AbstractValidator<UserResultDto>
	{
		public UserResultDtoValidator()
		{
			RuleFor(x => x.Success)
				.NotNull()
				.WithMessage("Success status is required.");

			RuleFor(x => x.Token)
				.MaximumLength(1000)
				.WithMessage("Token cannot exceed 1000 characters.");

			RuleFor(x => x.Message)
				.MaximumLength(500)
				.WithMessage("Message cannot exceed 500 characters.");
		}
	}
}