using FluentValidation;
using SocialMedia.Application.DTOs.Media;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.Validators
{
    public class GenerateUploadSignaturesRequestValidator : AbstractValidator<GenerateUploadSignaturesRequest>
    {
        private static readonly HashSet<MediaType> AllowedTypes =
            new() { MediaType.Image, MediaType.Gif, MediaType.Video };

        public GenerateUploadSignaturesRequestValidator()
        {
            RuleFor(x => x.Items)
                .NotNull().NotEmpty()
                    .WithMessage("At least one media item is required.")
                .Must(items => items.Count <= 3)
                    .WithMessage("A maximum of 3 media items are allowed per request.");

            RuleForEach(x => x.Items)
                .Must(item => AllowedTypes.Contains(item.FileType))
                    .WithMessage("Allowed file types are: Image, Gif, Video.");
        }
    }
}
