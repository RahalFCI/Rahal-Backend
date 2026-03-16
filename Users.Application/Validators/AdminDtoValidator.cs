using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Admin;

namespace Users.Application.Validators
{
    public class AdminDtoValidator : AbstractValidator<AdminDto>
    {
        public AdminDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty).WithMessage("Admin ID must be a valid GUID");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(3, 100).WithMessage("Name must be between 3 and 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be a valid international format (E.164)");

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
