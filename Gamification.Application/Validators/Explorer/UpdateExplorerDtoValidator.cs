using FluentValidation;
using Gamification.Application.DTOs.Explorer;
using Shared.Application.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Validators.Explorer
{
    public class UpdateExplorerDtoValidator : AbstractValidator<UpdateExplorerDto>
    {
        public UpdateExplorerDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty).WithMessage("Explorer ID must be a valid GUID");

            RuleFor(x => x.DisplayName)
                .MaximumLength(100).WithMessage("Display name must not exceed 100 characters");

            RuleFor(x => x.ProfilePictureUrl)
                .MaximumLength(100).WithMessage("Profile picture URL must not exceed 100 characters");

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Bio must not exceed 500 characters");

            RuleFor(x => x.IsPublic)
                .NotNull().WithMessage("IsPublic is required");

            RuleFor(x => x.IsPremium)
                .NotNull().WithMessage("IsPremium is required");

            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("Country code is required")
                .Length(2).WithMessage("Country code must be a 2-letter ISO 3166-1 code")
                .Must(code => CountryValidator.IsValid(code)).WithMessage("Country code must be a valid ISO 3166-1 country code");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .IsInEnum().WithMessage("Gender must be a valid enum value");

            RuleFor(x => x.Level)
                .GreaterThanOrEqualTo(0).WithMessage("Level must be greater than or equal to 0");

            RuleFor(x => x.CumlativeXp)
                .GreaterThanOrEqualTo(0).WithMessage("Cumulative XP must be greater than or equal to 0");

            RuleFor(x => x.AvailableXp)
                .GreaterThanOrEqualTo(0).WithMessage("Available XP must be greater than or equal to 0");

            RuleFor(x => x.BirthDate)
                .Must(date => date < DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Birth date must be in the past.");
        }
    }
}
