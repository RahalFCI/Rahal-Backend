using FluentValidation;
using Places.Application.DTOs.PlacePhoto;

namespace Places.Application.Validators
{
    public class AddPlacePhotoDtoValidator : AbstractValidator<AddPlacePhotoDto>
    {
        public AddPlacePhotoDtoValidator()
        {
            RuleFor(x => x.PlaceId)
                .NotEmpty().WithMessage("Place ID is required");

            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("Photo URL is required")
                .MaximumLength(500).WithMessage("Photo URL cannot exceed 500 characters");
        }
    }
}
