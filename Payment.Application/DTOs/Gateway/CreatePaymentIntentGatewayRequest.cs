namespace Payment.Application.DTOs.Gateway
{
    public record CreatePaymentIntentGatewayRequest(
        long AmountMinor,
        string Currency);
}
