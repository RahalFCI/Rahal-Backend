using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.OAuth;

namespace Users.Application.Validators
{
    public class GoogleSignInRequestValidator : AbstractValidator<GoogleSignInRequest>
    {
        public GoogleSignInRequestValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Token is required");
        }
    }
}
