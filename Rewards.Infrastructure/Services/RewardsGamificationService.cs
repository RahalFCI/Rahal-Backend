using MassTransit;
using Microsoft.Extensions.Logging;
using Rewards.Application.Interfaces;
using Shared.Application.DTOs;
using Shared.Application.Events.Payments;
using Shared.Domain.Enums;

namespace Rewards.Infrastructure.Services
{
    public class RewardsGamificationService : IRewardsGamificationService
    {
        private readonly IRequestClient<SpendXpRequest> _spendXpClient;
        private readonly IRequestClient<SetExplorerPremiumRequest> _setPremiumClient;
        private readonly ILogger<RewardsGamificationService> _logger;

        public RewardsGamificationService(
            IRequestClient<SpendXpRequest> spendXpClient,
            IRequestClient<SetExplorerPremiumRequest> setPremiumClient,
            ILogger<RewardsGamificationService> logger)
        {
            _spendXpClient = spendXpClient;
            _setPremiumClient = setPremiumClient;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> SpendXpAsync(Guid operationId, Guid explorerId, int amount, string sourceType, Guid referenceId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Requesting XP spend for explorer {ExplorerId}. Operation {OperationId}, Amount {Amount}, SourceType {SourceType}, ReferenceId {ReferenceId}",
                explorerId,
                operationId,
                amount,
                sourceType,
                referenceId);

            try
            {
                var response = await _spendXpClient.GetResponse<SpendXpResponse>(
                    new SpendXpRequest(operationId, explorerId, amount, sourceType, referenceId),
                    cancellationToken);

                if (!response.Message.IsSuccess)
                    _logger.LogWarning("XP spend rejected for explorer {ExplorerId}. Operation {OperationId}, ErrorCode {ErrorCode}", explorerId, operationId, response.Message.ErrorCode);
                else
                    _logger.LogInformation("XP spend completed for explorer {ExplorerId}. Operation {OperationId}", explorerId, operationId);

                return response.Message.IsSuccess
                    ? ApiResponse<string>.Success(response.Message.Message ?? "XP spent successfully")
                    : ApiResponse<string>.Failure(response.Message.ErrorCode);
            }
            catch (RequestTimeoutException ex)
            {
                _logger.LogError(ex, "XP spend timed out for explorer {ExplorerId}. Operation {OperationId}", explorerId, operationId);
                return ApiResponse<string>.Failure(ErrorCode.Timeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "XP spend failed for explorer {ExplorerId}. Operation {OperationId}", explorerId, operationId);
                return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
            }
        }

        public async Task<ApiResponse<string>> SetPremiumAsync(Guid operationId, Guid explorerId, bool isPremium, Guid? planTierId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Requesting premium update for explorer {ExplorerId}. Operation {OperationId}, IsPremium {IsPremium}, PlanTierId {PlanTierId}",
                explorerId,
                operationId,
                isPremium,
                planTierId);

            try
            {
                var response = await _setPremiumClient.GetResponse<SetExplorerPremiumResponse>(
                    new SetExplorerPremiumRequest(operationId, explorerId, isPremium),
                    cancellationToken);

                if (!response.Message.IsSuccess)
                    _logger.LogWarning("Premium update rejected for explorer {ExplorerId}. Operation {OperationId}, ErrorCode {ErrorCode}", explorerId, operationId, response.Message.ErrorCode);
                else
                    _logger.LogInformation("Premium update completed for explorer {ExplorerId}. Operation {OperationId}", explorerId, operationId);

                return response.Message.IsSuccess
                    ? ApiResponse<string>.Success(response.Message.Message ?? "Explorer premium state updated successfully")
                    : ApiResponse<string>.Failure(response.Message.ErrorCode);
            }
            catch (RequestTimeoutException ex)
            {
                _logger.LogError(ex, "Premium update timed out for explorer {ExplorerId}. Operation {OperationId}", explorerId, operationId);
                return ApiResponse<string>.Failure(ErrorCode.Timeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Premium update failed for explorer {ExplorerId}. Operation {OperationId}", explorerId, operationId);
                return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
            }
        }
    }
}
