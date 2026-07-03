using Payment.Domain.Enums;

namespace Payment.Application.DTOs.Gateway
{
    public record GatewayPaymentStatusResult(
        string PaymentIntentId,
        PaymentStatus Status,
        string? FailureMessage);
}
