using Shared.Application.DTOs;
using Places.Application.DTOs.PlacePhoto;
using Microsoft.AspNetCore.Http;

namespace Places.Application.Interfaces
{
    public interface IPlacePhotoService
    {
        Task<ApiResponse<GetPlacePhotoDto>> GetPlacePhotoAsync(Guid placeId, string url, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetPlacePhotoDto>>> GetPhotosByPlaceIdAsync(Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> AddPlacePhotoAsync(Guid placeId, IFormFile photo, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeletePlacePhotoAsync(Guid placeId, string url, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetPlacePhotoDto>>> GetPhotosByPlaceIdsAsync(IEnumerable<Guid> placeIds, CancellationToken cancellationToken = default);
    }
}
