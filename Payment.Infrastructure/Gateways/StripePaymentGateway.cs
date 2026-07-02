using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Application.DTOs.Gateway;
using Payment.Application.Interfaces;
using Payment.Domain.Enums;
using Payment.Infrastructure.Settings;
using Stripe;

namespace Payment.Infrastructure.Gateways
{
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly StripeSettings _settings;
        private readonly ILogger<StripePaymentGateway> _logger;

        public StripePaymentGateway(
            IOptions<StripeSettings> settings,
            ILogger<StripePaymentGateway> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public string GatewayName => PaymentGatewayType.Stripe.ToString();

        public string PublishableKey => _settings.PublishableKey;

        public async Task<CreatePaymentIntentGatewayResult> CreatePaymentIntentAsync(
            CreatePaymentIntentGatewayRequest request,
            CancellationToken cancellationToken = default)
        {
            EnsureConfigured(requireWebhookSecret: false);

            var client = new StripeClient(_settings.SecretKey);
            var customerService = new CustomerService(client);
            var ephemeralKeyService = new EphemeralKeyService(client);
            var paymentIntentService = new PaymentIntentService(client);

            var customer = await customerService.CreateAsync(
                new CustomerCreateOptions(),
                cancellationToken: cancellationToken);

            var ephemeralKey = await ephemeralKeyService.CreateAsync(
                new EphemeralKeyCreateOptions
                {
                    Customer = customer.Id
                },
                cancellationToken: cancellationToken);

            var paymentIntent = await paymentIntentService.CreateAsync(
                new PaymentIntentCreateOptions
                {
                    Amount = request.AmountMinor,
                    Currency = request.Currency,
                    Customer = customer.Id,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    }
                },
                cancellationToken: cancellationToken);

            return new CreatePaymentIntentGatewayResult(
                paymentIntent.Id,
                paymentIntent.ClientSecret,
                customer.Id,
                ephemeralKey.Secret,
                MapStatus(paymentIntent.Status));
        }

        public async Task<GatewayPaymentStatusResult> RetrievePaymentStatusAsync(
            string paymentIntentId,
            CancellationToken cancellationToken = default)
        {
            EnsureConfigured(requireWebhookSecret: false);

            var paymentIntent = await new PaymentIntentService(new StripeClient(_settings.SecretKey))
                .GetAsync(paymentIntentId, cancellationToken: cancellationToken);

            return new GatewayPaymentStatusResult(
                paymentIntent.Id,
                MapStatus(paymentIntent.Status),
                paymentIntent.LastPaymentError?.Message);
        }

        public GatewayWebhookPaymentResult? ParsePaymentWebhook(
            string payload,
            string signatureHeader)
        {
            EnsureConfigured(requireWebhookSecret: true);

            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signatureHeader,
                _settings.WebhookSecret,
                throwOnApiVersionMismatch: false);

            if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
            {
                return null;
            }

            if (!IsSupportedPaymentIntentEvent(stripeEvent.Type))
            {
                return null;
            }

            _logger.LogInformation(
                "Received Stripe payment webhook {EventType} for payment intent {PaymentIntentId}",
                stripeEvent.Type,
                paymentIntent.Id);

            return new GatewayWebhookPaymentResult(
                stripeEvent.Id,
                stripeEvent.Type,
                paymentIntent.Id,
                MapStatus(paymentIntent.Status),
                paymentIntent.LastPaymentError?.Message);
        }

        private void EnsureConfigured(bool requireWebhookSecret)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                throw new InvalidOperationException("Stripe secret key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_settings.PublishableKey))
            {
                throw new InvalidOperationException("Stripe publishable key is not configured.");
            }

            if (requireWebhookSecret && string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            {
                throw new InvalidOperationException("Stripe webhook secret is not configured.");
            }
        }

        private static bool IsSupportedPaymentIntentEvent(string eventType) =>
            eventType is "payment_intent.succeeded"
                or "payment_intent.payment_failed"
                or "payment_intent.canceled"
                or "payment_intent.processing"
                or "payment_intent.requires_action";

        private static PaymentStatus MapStatus(string? status) =>
            status switch
            {
                "requires_payment_method" => PaymentStatus.RequiresPaymentMethod,
                "requires_action" => PaymentStatus.RequiresAction,
                "processing" => PaymentStatus.Processing,
                "succeeded" => PaymentStatus.Succeeded,
                "canceled" => PaymentStatus.Canceled,
                _ => PaymentStatus.Pending
            };
    }
}
