using Shared.Domain.Enums;

namespace Shared.Application.Events.Gamification
{
    public record GetExplorerPaymentProfilesResponse(
        bool IsSuccess,
        ErrorCode ErrorCode,
        IReadOnlyList<ExplorerPaymentProfileDto> Explorers);
}
