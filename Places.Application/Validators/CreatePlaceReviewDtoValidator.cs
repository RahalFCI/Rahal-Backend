using FluentValidation;
using Places.Application.DTOs.PlaceReview;

namespace Places.Application.Validators
{
    public class CreatePlaceReviewDtoValidator : AbstractValidator<CreatePlaceReviewDto>
    {
        public CreatePlaceReviewDtoValidator()
        {
            RuleFor(x => x.ExplorerId)
                .NotEmpty().WithMessage("Explorer ID is required");

            RuleFor(x => x.PlaceId)
                .NotEmpty().WithMessage("Place ID is required");

            RuleFor(x => x.CheckInId)
                .NotEmpty().WithMessage("Check-in ID is required");

            RuleFor(x => x.Rating)
                .GreaterThan(0).WithMessage("Rating must be greater than 0")
                .LessThanOrEqualTo(5).WithMessage("Rating cannot exceed 5");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Comment is required")
                .MinimumLength(5).WithMessage("Comment must be at least 5 characters");
        }
    }
}
