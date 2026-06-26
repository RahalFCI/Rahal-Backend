using FluentValidation;
using Gamification.Application.DTOs.VendorBranches;

namespace Gamification.Application.Validators.VendorBranches
{
    public class UpdateVendorBranchDtoValidator : AbstractValidator<UpdateVendorBranchDto>
    {
        public UpdateVendorBranchDtoValidator()
        {
            RuleFor(x => x.BranchName)
                .NotEmpty().WithMessage("Branch name is required")
                .MaximumLength(100).WithMessage("Branch name must not exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(30).WithMessage("Phone number must not exceed 30 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.GeoFenceRange)
                .GreaterThan(0).WithMessage("Geofence range must be greater than zero");
        }
    }
}
