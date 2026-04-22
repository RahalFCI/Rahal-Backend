using Shared.Application.DTOs;
using Places.Application.DTOs.PlaceReview;

namespace Places.Application.Interfaces
{
    public interface IPlaceReviewService
    {
        Task<ApiResponse<GetPlaceReviewDto>> GetPlaceReviewAsync(Guid explorerId, Guid placeId, Guid checkInId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetPlaceReviewDto>>> GetReviewsByPlaceIdAsync(Guid placeId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetPlaceReviewDto>>> GetReviewsByExplorerIdAsync(Guid explorerId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> CreatePlaceReviewAsync(CreatePlaceReviewDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdatePlaceReviewAsync(Guid explorerId, Guid placeId, Guid checkInId, UpdatePlaceReviewDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeletePlaceReviewAsync(Guid explorerId, Guid placeId, Guid checkInId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<GetPlaceReviewDto>>> GetVerifiedReviewsByPlaceIdAsync(Guid placeId, CancellationToken cancellationToken = default);
    }
}
