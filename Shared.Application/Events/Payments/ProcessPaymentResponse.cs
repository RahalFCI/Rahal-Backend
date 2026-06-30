using Shared.Domain.Enums;

namespace Shared.Application.Events.Payments
{
    public record ProcessPaymentResponse(
        Guid OperationId,
        bool IsSuccess,
        ErrorCode ErrorCode,
        string? TransactionId,
        string? Message);
}
