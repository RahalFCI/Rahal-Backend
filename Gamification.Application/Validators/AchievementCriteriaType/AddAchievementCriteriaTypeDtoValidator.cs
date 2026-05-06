using FluentValidation;
using Gamification.Application.DTOs.AchievementCriteriaType;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Validators.AchievementCriteriaType
{
    public class AddAchievementCriteriaTypeDtoValidator : AbstractValidator<AddAchievementCriteriaTypeDto>
    {
        public AddAchievementCriteriaTypeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
        }
    }
}
