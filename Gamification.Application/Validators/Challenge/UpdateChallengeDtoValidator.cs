using FluentValidation;
using Gamification.Application.DTOs.Challenge;

namespace Gamification.Application.Validators.Challenge
{
    public class UpdateChallengeDtoValidator : AbstractValidator<UpdateChallengeDto>
    {
        public UpdateChallengeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Challenge name is required")
                .MaximumLength(200).WithMessage("Challenge name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Challenge description is required")
                .MaximumLength(500).WithMessage("Challenge description cannot exceed 500 characters");

            RuleFor(x => x.Difficulty)
                .NotEmpty().WithMessage("Challenge difficulty is required");

            RuleFor(x => x.MinimumLevelRequired)
                .GreaterThan(0).WithMessage("Minimum level required must be greater than 0");

            RuleFor(x => x.XpReward)
                .GreaterThan(0).WithMessage("XP reward must be greater than 0");
        }
    }
}
