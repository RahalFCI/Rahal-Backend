using Payment.Domain.Enums;

namespace Payment.Application.DTOs.Gateway
{
    public record CreatePaymentIntentGatewayResult(
        string PaymentIntentId,
        string PaymentIntentClientSecret,
        string? CustomerId,
        string? EphemeralKeySecret,
        PaymentStatus Status);
}
