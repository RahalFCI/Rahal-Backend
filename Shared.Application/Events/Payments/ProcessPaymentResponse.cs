using Shared.Domain.Enums;

namespace Shared.Application.Events.Payments
{
    public record ProcessPaymentResponse(
        Guid OperationId,
        bool IsSuccess,
        ErrorCode ErrorCode,
        string? TransactionId,
        string? Message,
        string? PaymentIntentClientSecret = null,
        string? CustomerId = null,
        string? EphemeralKeySecret = null,
        string? PublishableKey = null);
}
