using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Auth;

namespace Users.Application.Validators
{
    public class AuthRequestDtoValidator : AbstractValidator<AuthRequestDto>
    {
        public AuthRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long");
        }
    }
}
