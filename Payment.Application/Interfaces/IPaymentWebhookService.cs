using Payment.Application.DTOs.Webhooks;

namespace Payment.Application.Interfaces
{
    public interface IPaymentWebhookService
    {
        Task<PaymentWebhookHandleResult> HandleGatewayWebhookAsync(
            string payload,
            string signatureHeader,
            CancellationToken cancellationToken = default);
    }
}
