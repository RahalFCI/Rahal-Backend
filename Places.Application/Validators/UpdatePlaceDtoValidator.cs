using FluentValidation;
using Places.Application.DTOs.Place;

namespace Places.Application.Validators
{
    public class UpdatePlaceDtoValidator : AbstractValidator<UpdatePlaceDto>
    {
        public UpdatePlaceDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Place name is required")
                .MaximumLength(200).WithMessage("Place name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Place description is required");

            RuleFor(x => x.PlaceCategoryId)
                .NotEmpty().WithMessage("Place category is required");

            RuleFor(x => x.TicketPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Ticket price cannot be negative");

            RuleFor(x => x.Latitude)
                .GreaterThanOrEqualTo(-90).WithMessage("Latitude must be between -90 and 90")
                .LessThanOrEqualTo(90).WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .GreaterThanOrEqualTo(-180).WithMessage("Longitude must be between -180 and 180")
                .LessThanOrEqualTo(180).WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.GeoFenceRange)
                .GreaterThan(0).WithMessage("Geofence range must be greater than 0");

            RuleFor(x => x.Address)
                .NotNull().WithMessage("Address is required")
                .SetValidator(new AddressDtoValidator()!);
        }
    }
}
