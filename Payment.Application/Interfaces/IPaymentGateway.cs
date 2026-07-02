using Payment.Application.DTOs.Gateway;

namespace Payment.Application.Interfaces
{
    public interface IPaymentGateway
    {
        string GatewayName { get; }

        string PublishableKey { get; }

        Task<CreatePaymentIntentGatewayResult> CreatePaymentIntentAsync(
            CreatePaymentIntentGatewayRequest request,
            CancellationToken cancellationToken = default);

        Task<GatewayPaymentStatusResult> RetrievePaymentStatusAsync(
            string paymentIntentId,
            CancellationToken cancellationToken = default);

        GatewayWebhookPaymentResult? ParsePaymentWebhook(
            string payload,
            string signatureHeader);
    }
}
