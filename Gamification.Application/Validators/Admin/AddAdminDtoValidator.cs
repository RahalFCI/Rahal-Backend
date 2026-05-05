using FluentValidation;
using Gamification.Application.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Validators.Admin
{
    public class AddAdminDtoValidator : AbstractValidator<AddAdminDto>
    {
        public AddAdminDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty).WithMessage("Admin ID must be a valid GUID");
        }
    }
}
