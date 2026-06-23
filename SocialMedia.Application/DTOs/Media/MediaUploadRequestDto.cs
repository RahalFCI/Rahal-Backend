using SocialMedia.Domain.Enums;

namespace SocialMedia.Application.DTOs.Media
{
    /// <summary>
    /// One item in the request body — the client says "I want to upload this type of file".
    /// </summary>
    public class MediaUploadRequestItem
    {
        public MediaType FileType { get; set; }
    }

    /// <summary>
    /// The full request payload: up to 3 items.
    /// </summary>
    public class GenerateUploadSignaturesRequest
    {
        public List<MediaUploadRequestItem> Items { get; set; } = new();
    }
}
