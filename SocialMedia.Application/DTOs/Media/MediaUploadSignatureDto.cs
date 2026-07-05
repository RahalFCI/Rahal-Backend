namespace SocialMedia.Application.DTOs.Media
{
    /// <summary>
    /// Returned to the client for each media item.
    /// The client uses these values to upload directly to Cloudinary.
    /// </summary>
    public class MediaUploadSignatureItem
    {
        /// <summary>The unique ID pre-assigned to this upload slot.</summary>
        public string PublicId { get; set; } = string.Empty;

        /// <summary>HMAC-SHA1 signature the client passes to Cloudinary.</summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>Unix timestamp used when the signature was generated.</summary>
        public long Timestamp { get; set; }

        /// <summary>Cloudinary API key (public — safe to send to client).</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Cloudinary cloud name (public — safe to send to client).</summary>
        public string CloudName { get; set; } = string.Empty;
    }

    /// <summary>Full response from POST /api/media/signatures.</summary>
    public class GenerateUploadSignaturesResponse
    {
        public List<MediaUploadSignatureItem> Signatures { get; set; } = new();
    }
}
