using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs.Register;

namespace Users.Application.Validators
{
    public class RegisterAdminDtoValidator : AbstractValidator<RegisterAdminDto>
    {
        public RegisterAdminDtoValidator()
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

            RuleFor(x => x.ProfilePictureUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.ProfilePictureUrl))
                .WithMessage("Profile picture URL must be a valid URL");
        }
    }
}
