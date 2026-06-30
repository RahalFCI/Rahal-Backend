using Rewards.Application.DTOs.PlanTiers;
using Shared.Application.DTOs;
using Shared.Application.Pagination;

namespace Rewards.Application.Interfaces
{
    public interface IPlanTierService
    {
        Task<ApiResponse<GetPlanTierDto>> CreateAsync(CreatePlanTierDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<GetPlanTierDto>> UpdateAsync(Guid id, UpdatePlanTierDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<GetPlanTierDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetPlanTierDto>>> GetAllAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> PermanentDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
