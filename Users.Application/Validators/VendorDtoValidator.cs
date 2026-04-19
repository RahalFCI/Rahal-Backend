using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Vendor;

namespace Users.Application.Validators
{
    public class VendorDtoValidator : AbstractValidator<VendorDto>
    {
        public VendorDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty).WithMessage("Vendor ID must be a valid GUID");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(3, 100).WithMessage("Name must be between 3 and 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be a valid international format (E.164)");

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

            RuleFor(x => x.ProfilePictureUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.ProfilePictureUrl))
                .WithMessage("Profile picture URL must be a valid URL");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .IsInEnum().WithMessage("Role must be a valid enum value");
        }
    }
}
