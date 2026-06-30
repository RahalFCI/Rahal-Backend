using Shared.Domain.Enums;

namespace Shared.Application.Events.Payments
{
    public record SpendXpResponse(
        Guid OperationId,
        bool IsSuccess,
        ErrorCode ErrorCode,
        string? Message);
}
