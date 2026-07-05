using FluentValidation;
using SocialMedia.Application.DTOs.Comments;

namespace SocialMedia.Application.Validators
{
    public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
    {
        public CreateCommentRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Comment content cannot be empty.");
        }
    }

    public class EditCommentRequestValidator : AbstractValidator<EditCommentRequest>
    {
        public EditCommentRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Comment content cannot be empty.");
        }
    }
}
