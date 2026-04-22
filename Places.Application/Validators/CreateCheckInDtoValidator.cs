using FluentValidation;
using Places.Application.DTOs.CheckIn;

namespace Places.Application.Validators
{
    public class CreateCheckInDtoValidator : AbstractValidator<CreateCheckInDto>
    {
        public CreateCheckInDtoValidator()
        {
            RuleFor(x => x.ExplorerId)
                .NotEmpty().WithMessage("Explorer ID is required");

            RuleFor(x => x.PlaceId)
                .NotEmpty().WithMessage("Place ID is required");
        }
    }
}
