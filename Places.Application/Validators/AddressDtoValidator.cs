using FluentValidation;
using Places.Application.DTOs.Place;

namespace Places.Application.Validators
{
    public class AddressDtoValidator : AbstractValidator<AddressDto>
    {
        public AddressDtoValidator()
        {
            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Address line is required")
                .MaximumLength(500).WithMessage("Address line cannot exceed 500 characters");

            RuleFor(x => x.Government)
                .NotEmpty().WithMessage("Government/State is required")
                .MaximumLength(100).WithMessage("Government/State cannot exceed 100 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");
        }
    }
}
