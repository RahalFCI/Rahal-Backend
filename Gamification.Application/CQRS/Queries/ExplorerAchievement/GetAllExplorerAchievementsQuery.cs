using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;

namespace Gamification.Application.CQRS.Queries.ExplorerAchievement
{
    public record GetAllExplorerAchievementsQuery(OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<DTOs.ExplorerAchievement.GetExplorerAchievementDto>>>;
}
