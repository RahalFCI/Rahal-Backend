using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Auth;

namespace Users.Application.Validators
{
    public class TokenDtoValidator : AbstractValidator<TokenDto>
    {
        public TokenDtoValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Token is required.")
                .MinimumLength(10).WithMessage("Token must be at least 10 characters long.");
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token is required.")
                .MinimumLength(10).WithMessage("Access token must be at least 10 characters long.");
        }
    }
}
