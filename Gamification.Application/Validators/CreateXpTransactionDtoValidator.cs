using FluentValidation;
using Gamification.Application.DTOs.XpTransaction;

namespace Gamification.Application.Validators
{
    public class CreateXpTransactionDtoValidator : AbstractValidator<CreateXpTransactionDto>
    {
        public CreateXpTransactionDtoValidator()
        {
            RuleFor(x => x.ExplorerId)
                .NotEmpty().WithMessage("Explorer ID is required");

            RuleFor(x => x.SourceType)
                .NotEmpty().WithMessage("Source type is required");

            RuleFor(x => x.ReferenceId)
                .NotEmpty().WithMessage("Reference ID type is required");
        }
    }
}
