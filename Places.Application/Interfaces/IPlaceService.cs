using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Places.Application.DTOs.Place;

namespace Places.Application.Interfaces
{
    public interface IPlaceService
    {
        Task<ApiResponse<GetPlaceDto>> GetPlaceByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetPlaceDto>>> GetAllPlacesAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetPlaceDto>>> GetPlacesByCategoryIdAsync(Guid categoryId, OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> CreatePlaceAsync(CreatePlaceDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdatePlaceAsync(Guid id, UpdatePlaceDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeletePlaceAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeletePlacePermanentlyAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetPlaceDto>>> SearchPlacesByLocationAsync(double latitude, double longitude, int radiusInMeters, OffsetPaginationRequest request, CancellationToken cancellationToken = default);
    }
}
