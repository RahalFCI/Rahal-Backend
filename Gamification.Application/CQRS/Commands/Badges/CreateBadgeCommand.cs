using Gamification.Application.DTOs.Badge;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.Badges
{
    public record CreateBadgeCommand(CreateBadgeDto Dto) : IRequest<ApiResponse<GetBadgeDto>>;

}
