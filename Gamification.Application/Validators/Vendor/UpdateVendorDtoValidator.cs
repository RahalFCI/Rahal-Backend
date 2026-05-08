using FluentValidation;
using Gamification.Application.DTOs.Vendor;
using Shared.Application.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Validators.Vendor
{
    public class UpdateVendorDtoValidator : AbstractValidator<UpdateVendorDto>
    {
        public UpdateVendorDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.CountryCode)
                    .NotEmpty().WithMessage("Country code is required")
                    .Length(2).WithMessage("Country code must be a 2-letter ISO 3166-1 code")
                    .Must(code => CountryValidator.IsValid(code)).WithMessage("Country code must be a valid ISO 3166-1 country code");

            RuleFor(x => x.Address)
                    .NotEmpty().WithMessage("Address is required")
                    .Length(5, 200).WithMessage("Address must be between 5 and 200 characters");

            RuleFor(x => x.AddressUrl)
                    .NotEmpty().WithMessage("Address URL is required")
                    .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                    .WithMessage("Address URL must be a valid URL");

            RuleFor(x => x.WorkingHours)
                    .NotNull().WithMessage("Working hours are required")
                    .Must(wh => wh != null && wh.Count > 0).WithMessage("At least one working day must be specified");

            RuleFor(x => x.CategoryId)
                    .NotNull().WithMessage("Category is required");
        }
        
    }
}
