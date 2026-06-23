using Shared.Application.DTOs;
using SocialMedia.Application.DTOs.Media;

namespace SocialMedia.Application.Interfaces
{
    /// <summary>
    /// Application service: orchestrates signature generation + Redis state registration.
    /// </summary>
    public interface IMediaService
    {
        Task<ApiResponse<GenerateUploadSignaturesResponse>> GenerateUploadSignaturesAsync(
            GenerateUploadSignaturesRequest request,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
