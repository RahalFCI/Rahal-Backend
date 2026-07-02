using Microsoft.Extensions.Logging;
using Payment.Application.DTOs.Gateway;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Shared.Application.Events.Payments;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Payment.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly IGenericRepository<PaymentTransaction> _paymentRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentGateway paymentGateway,
            IGenericRepository<PaymentTransaction> paymentRepository,
            ILogger<PaymentService> logger)
        {
            _paymentGateway = paymentGateway;
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<ProcessPaymentResponse> ProcessPaymentAsync(
            ProcessPaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidRequest(request, out var validationMessage))
            {
                return Failure(request, ErrorCode.ValidationError, validationMessage);
            }

            var currency = request.Currency.Trim().ToLowerInvariant();
            var payment = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                OperationId = request.OperationId,
                ExplorerId = request.ExplorerId,
                ReferenceId = request.ReferenceId,
                Amount = request.Amount,
                AmountMinor = ToMinorUnits(request.Amount, currency),
                Currency = currency,
                Gateway = PaymentGatewayType.Stripe,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _paymentRepository.Add(payment);

            try
            {
                var gatewayResult = await _paymentGateway.CreatePaymentIntentAsync(
                    new CreatePaymentIntentGatewayRequest(
                        payment.AmountMinor,
                        payment.Currency),
                    cancellationToken);

                payment.GatewayPaymentIntentId = gatewayResult.PaymentIntentId;
                payment.GatewayCustomerId = gatewayResult.CustomerId;
                payment.Status = gatewayResult.Status;
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.SaveChangesAsync(cancellationToken);

                return new ProcessPaymentResponse(
                    request.OperationId,
                    true,
                    ErrorCode.None,
                    payment.Id.ToString(),
                    "Payment intent created successfully.",
                    gatewayResult.PaymentIntentClientSecret,
                    gatewayResult.CustomerId,
                    gatewayResult.EphemeralKeySecret,
                    _paymentGateway.PublishableKey);
            }
            catch (Exception ex)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureMessage = ex.Message;
                payment.UpdatedAt = DateTime.UtcNow;

                try
                {
                    await _paymentRepository.SaveChangesAsync(cancellationToken);
                }
                catch (Exception saveException)
                {
                    _logger.LogError(
                        saveException,
                        "Failed to persist failed payment state for operation {OperationId}",
                        request.OperationId);
                }

                _logger.LogError(
                    ex,
                    "Payment processing failed for operation {OperationId} and reference {ReferenceId}",
                    request.OperationId,
                    request.ReferenceId);

                return Failure(request, ErrorCode.ExternalServiceError, "Failed to create payment intent.");
            }
        }

        private static bool IsValidRequest(ProcessPaymentRequest request, out string message)
        {
            if (request.OperationId == Guid.Empty)
            {
                message = "OperationId is required.";
                return false;
            }

            if (request.ExplorerId == Guid.Empty)
            {
                message = "ExplorerId is required.";
                return false;
            }

            if (request.ReferenceId == Guid.Empty)
            {
                message = "ReferenceId is required.";
                return false;
            }

            if (request.Amount <= 0)
            {
                message = "Amount must be greater than zero.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Currency))
            {
                message = "Currency is required.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static long ToMinorUnits(decimal amount, string currency)
        {
            var zeroDecimalCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf",
                "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
            };

            var multiplier = zeroDecimalCurrencies.Contains(currency) ? 1 : 100;
            return decimal.ToInt64(decimal.Round(amount * multiplier, 0, MidpointRounding.AwayFromZero));
        }

        private static ProcessPaymentResponse Failure(
            ProcessPaymentRequest request,
            ErrorCode errorCode,
            string? message) =>
            new(
                request.OperationId,
                false,
                errorCode,
                null,
                message);
    }
}
