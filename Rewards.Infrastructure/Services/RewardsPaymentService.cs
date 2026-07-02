using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rewards.Application.Interfaces;
using Shared.Application.DTOs;
using Shared.Application.Events.Payments;
using Shared.Domain.Enums;

namespace Rewards.Infrastructure.Services
{
    public class RewardsPaymentService : IRewardsPaymentService
    {
        private readonly IRequestClient<ProcessPaymentRequest> _processPaymentClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RewardsPaymentService> _logger;

        public RewardsPaymentService(
            IRequestClient<ProcessPaymentRequest> processPaymentClient,
            IConfiguration configuration,
            ILogger<RewardsPaymentService> logger)
        {
            _processPaymentClient = processPaymentClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> ProcessPaymentAsync(
            Guid operationId,
            Guid explorerId,
            decimal amount,
            string paymentMethod,
            Guid referenceId,
            CancellationToken cancellationToken = default)
        {
            var currency = _configuration["Rewards:Subscriptions:PaymentCurrency"];
            if (string.IsNullOrWhiteSpace(currency))
            {
                _logger.LogError("Subscription payment currency is not configured");
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }

            _logger.LogInformation(
                "Requesting payment for explorer {ExplorerId}. Operation {OperationId}, Amount {Amount}, Currency {Currency}, PaymentMethod {PaymentMethod}, ReferenceId {ReferenceId}",
                explorerId,
                operationId,
                amount,
                currency,
                paymentMethod,
                referenceId);

            try
            {
                var response = await _processPaymentClient.GetResponse<ProcessPaymentResponse>(
                    new ProcessPaymentRequest(operationId, explorerId, amount, currency, paymentMethod, referenceId),
                    cancellationToken);

                if (!response.Message.IsSuccess)
                    _logger.LogWarning("Payment rejected for explorer {ExplorerId}. Operation {OperationId}, ErrorCode {ErrorCode}", explorerId, operationId, response.Message.ErrorCode);
                else
                    _logger.LogInformation("Payment completed for explorer {ExplorerId}. Operation {OperationId}, TransactionId {TransactionId}", explorerId, operationId, response.Message.TransactionId);

                return response.Message.IsSuccess
                    ? ApiResponse<string>.Success(response.Message.TransactionId ?? response.Message.Message ?? "Payment processed successfully")
                    : ApiResponse<string>.Failure(response.Message.ErrorCode);
            }
            catch (RequestTimeoutException ex)
            {
                _logger.LogError(ex, "Payment request timed out for explorer {ExplorerId}. Operation {OperationId}", explorerId, operationId);
                return ApiResponse<string>.Failure(ErrorCode.Timeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment request failed for explorer {ExplorerId}. Operation {OperationId}", explorerId, operationId);
                return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
            }
        }
    }
}
