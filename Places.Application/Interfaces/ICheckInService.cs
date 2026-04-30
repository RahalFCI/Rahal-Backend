using Shared.Application.DTOs;
using Places.Application.DTOs.CheckIn;

namespace Places.Application.Interfaces
{
    public interface ICheckInService
    {
        Task<ApiResponse<string>> CheckInAsync(Guid explorerId, CheckInRequestDto request, CancellationToken ct = default);
        Task<ApiResponse<GetCheckInDto>> GetCheckInAsync(Guid explorerId, Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetCheckInDto>>> GetAllCheckInAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetCheckInDto>>> GetCheckInsByPlaceIdAsync(Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetCheckInDto>>> GetCheckInsByExplorerIdAsync(Guid explorerId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdateCheckInStatusAsync(Guid explorerId, Guid placeId, UpdateCheckInDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteCheckInAsync(Guid explorerId, Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteCheckInPermanentlyAsync(Guid explorerId, Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetCheckInDto>>> GetPendingCheckInsAsync(CancellationToken cancellationToken = default);
    }
}
