using Shared.Domain.Enums;

namespace Shared.Application.Events.Payments
{
    public record SetExplorerPremiumResponse(
        Guid OperationId,
        bool IsSuccess,
        ErrorCode ErrorCode,
        string? Message);
}
