using Payment.Domain.Enums;

namespace Payment.Application.DTOs.Gateway
{
    public record GatewayWebhookPaymentResult(
        string EventId,
        string EventType,
        string PaymentIntentId,
        PaymentStatus Status,
        string? FailureMessage);
}
