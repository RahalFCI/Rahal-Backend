using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Explorer;

namespace Users.Application.Validators
{
    public class ExplorerDtoValidator : AbstractValidator<ExplorerDto>
    {
        public ExplorerDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty).WithMessage("Explorer ID must be a valid GUID");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(3, 100).WithMessage("Name must be between 3 and 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be a valid international format (E.164)");

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Bio must not exceed 500 characters");

            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("Country code is required")
                .Length(2).WithMessage("Country code must be a 2-letter ISO 3166-1 code")
                .Must(code => CountryValidator.IsValid(code)).WithMessage("Country code must be a valid ISO 3166-1 country code");

            RuleFor(x => x.AvailableXp)
                .GreaterThanOrEqualTo(0).WithMessage("Available XP cannot be negative");

            RuleFor(x => x.CumlativeXp)
                .GreaterThanOrEqualTo(0).WithMessage("Cumulative XP cannot be negative");

            RuleFor(x => x.Level)
                .GreaterThan(0).WithMessage("Level must be greater than 0");

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
