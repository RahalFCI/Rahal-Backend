using FluentValidation;
using Gamification.Application.DTOs.CheckInChallenge;

namespace Gamification.Application.Validators.CheckInChallenge
{
    public class CreateCheckInChallengeDtoValidator : AbstractValidator<CreateCheckInChallengeDto>
    {
        public CreateCheckInChallengeDtoValidator()
        {
            RuleFor(x => x.ChallengeId)
                .NotEmpty().WithMessage("Challenge ID is required");

            RuleFor(x => x.CheckInId)
                .NotEmpty().WithMessage("Check-in ID is required");

            RuleFor(x => x.ProofMediaUrl)
                .MaximumLength(500).WithMessage("Proof media URL cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ProofMediaUrl));
        }
    }
}
