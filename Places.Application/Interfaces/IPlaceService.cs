using Shared.Application.DTOs;
using Places.Application.DTOs.Place;

namespace Places.Application.Interfaces
{
    public interface IPlaceService
    {
        Task<ApiResponse<GetPlaceDto>> GetPlaceByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetPlaceDto>>> GetAllPlacesAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetPlaceDto>>> GetPlacesByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> CreatePlaceAsync(CreatePlaceDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdatePlaceAsync(Guid id, UpdatePlaceDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeletePlaceAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetPlaceDto>>> SearchPlacesByLocationAsync(double latitude, double longitude, int radiusInMeters, CancellationToken cancellationToken = default);
    }
}
