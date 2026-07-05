namespace SocialMedia.Application.DTOs.Posts
{
    public class CreatePostRequest
    {
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Cloudinary public_ids that were pre-signed via POST /api/media/signatures.
        /// The service validates them against the Redis pending_media:{userId} set before saving.
        /// </summary>
        public List<string> MediaIds { get; set; } = new();

        public bool IsPublic { get; set; } = true;
    }
}
