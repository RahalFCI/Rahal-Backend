using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs.Webhooks;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Shared.Application.Events.Payments;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Payment.Application.Services
{
    public class PaymentWebhookService : IPaymentWebhookService
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly IGenericRepository<PaymentTransaction> _paymentRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentWebhookService> _logger;

        public PaymentWebhookService(
            IPaymentGateway paymentGateway,
            IGenericRepository<PaymentTransaction> paymentRepository,
            IPublishEndpoint publishEndpoint,
            ILogger<PaymentWebhookService> logger)
        {
            _paymentGateway = paymentGateway;
            _paymentRepository = paymentRepository;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<PaymentWebhookHandleResult> HandleGatewayWebhookAsync(
            string payload,
            string signatureHeader,
            CancellationToken cancellationToken = default)
        {
            var webhookResult = _paymentGateway.ParsePaymentWebhook(payload, signatureHeader);
            if (webhookResult is null)
            {
                return PaymentWebhookHandleResult.Success("Webhook event ignored.");
            }

            var payment = await _paymentRepository.GetByExpression(
                payment => payment.GatewayPaymentIntentId == webhookResult.PaymentIntentId,
                cancellationToken);

            if (payment is null)
            {
                _logger.LogWarning(
                    "Payment webhook {EventId} referenced unknown payment intent {PaymentIntentId}",
                    webhookResult.EventId,
                    webhookResult.PaymentIntentId);

                return PaymentWebhookHandleResult.Failure(ErrorCode.NotFound, "Payment was not found.");
            }

            payment.Status = webhookResult.Status;
            payment.FailureMessage = webhookResult.FailureMessage;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new PaymentHandled(
                    payment.Id,
                    payment.OperationId,
                    payment.ExplorerId,
                    payment.ReferenceId,
                    payment.Amount,
                    payment.Currency,
                    payment.Status.ToString(),
                    payment.Gateway.ToString(),
                    payment.GatewayPaymentIntentId,
                    DateTime.UtcNow),
                cancellationToken);

            return PaymentWebhookHandleResult.Success("Payment webhook handled.");
        }
    }
}
