using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Places.Application.DTOs.CheckIn;

namespace Places.Application.Interfaces
{
    public interface ICheckInService
    {
        Task<ApiResponse<string>> CheckInAsync(Guid explorerId, CheckInRequestDto request, CancellationToken ct = default);
        Task<ApiResponse<GetCheckInDto>> GetCheckInAsync(Guid explorerId, Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetCheckInDto>>> GetAllCheckInAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetCheckInDto>>> GetCheckInsByPlaceIdAsync(Guid placeId, OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetCheckInDto>>> GetCheckInsByExplorerIdAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdateCheckInStatusAsync(Guid explorerId, Guid placeId, UpdateCheckInDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteCheckInAsync(Guid explorerId, Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteCheckInPermanentlyAsync(Guid explorerId, Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetCheckInDto>>> GetPendingCheckInsAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
    }
}
