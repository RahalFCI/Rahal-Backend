using FluentValidation;
using Places.Application.DTOs.PlaceReview;

namespace Places.Application.Validators
{
    public class UpdatePlaceReviewDtoValidator : AbstractValidator<UpdatePlaceReviewDto>
    {
        public UpdatePlaceReviewDtoValidator()
        {
            RuleFor(x => x.Rating)
                .GreaterThan(0).WithMessage("Rating must be greater than 0")
                .LessThanOrEqualTo(5).WithMessage("Rating cannot exceed 5");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Comment is required")
                .MinimumLength(5).WithMessage("Comment must be at least 5 characters");
        }
    }
}
