using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Auth;

namespace Users.Application.Validators
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");
        }
    }
}
