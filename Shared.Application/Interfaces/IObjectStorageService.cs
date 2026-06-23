namespace Shared.Application.Interfaces
{
    /// <summary>
    /// Abstraction over an object-storage provider (Cloudinary, AWS S3, DigitalOcean Spaces, etc.).
    /// Lives in Shared so any module can request signed uploads without depending on a specific provider.
    /// The interface uses provider-agnostic primitives (string, long) so it has no dependency on
    /// any module-specific domain enums (e.g. SocialMedia.Domain.Enums.MediaType).
    /// </summary>
    public interface IObjectStorageService
    {
        /// <summary>
        /// Generates a signed upload credential set that the client submits directly
        /// to the storage provider — the file never touches our servers.
        /// </summary>
        /// <param name="publicId">The pre-assigned unique ID for this upload slot.</param>
        /// <param name="resourceType">Provider resource type string, e.g. "image" or "video".</param>
        /// <param name="maxFileSize">Maximum allowed file size in bytes.</param>
        /// <returns>
        /// A tuple: Signature, Timestamp (Unix seconds), ApiKey, CloudName/bucket identifier.
        /// </returns>
        (string Signature, long Timestamp, string ApiKey, string CloudName) GenerateUploadSignature(
            string publicId,
            string resourceType,
            long maxFileSize);

        /// <summary>
        /// Reconstructs the full HTTPS delivery URL for a given publicId.
        /// </summary>
        string BuildMediaUrl(string publicId);
    }
}
