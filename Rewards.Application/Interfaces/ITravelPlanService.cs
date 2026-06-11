using Rewards.Application.DTOs.TravelPlans;
using Shared.Application.DTOs;
using Shared.Application.Pagination;

namespace Rewards.Application.Interfaces
{
    public interface ITravelPlanService
    {
        Task<ApiResponse<GetTravelPlanDto>> CreateAsync(Guid explorerId, CreateTravelPlanDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<GetTravelPlanDto>> GetByIdAsync(Guid explorerId, Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetTravelPlanDto>>> GetByExplorerAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default);
    }
}
