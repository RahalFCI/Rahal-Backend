using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs.Register;

namespace Users.Application.Validators
{
    public class RegisterVendorDtoValidator : AbstractValidator<RegisterVendorDto>
    {
        public RegisterVendorDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(3, 100).WithMessage("Name must be between 3 and 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit")
                .Matches(@"[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|`~]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm Password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");

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
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.AddressUrl))
                .WithMessage("Address URL must be a valid URL");

            RuleFor(x => x.WorkingHours)
                .NotNull().WithMessage("Working hours are required")
                .Must(wh => wh != null && wh.Count > 0).WithMessage("At least one working day must be specified");

            RuleFor(x => x.CategoryId)
                .NotEqual(0).WithMessage("Category is required")
                .GreaterThan(0).WithMessage("Category ID must be a positive number");

        }
    }
}
