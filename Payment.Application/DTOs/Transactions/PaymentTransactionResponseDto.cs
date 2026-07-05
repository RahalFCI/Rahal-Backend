using Payment.Domain.Enums;

namespace Payment.Application.DTOs.Transactions
{
    public record PaymentTransactionResponseDto(
        Guid TransactionId,
        Guid ExplorerId,
        string ExplorerDisplayName,
        Guid OperationId,
        Guid ReferenceId,
        decimal Amount,
        string Currency,
        PaymentStatus Status,
        PaymentGatewayType Gateway,
        string? GatewayPaymentIntentId,
        string? FailureMessage,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
