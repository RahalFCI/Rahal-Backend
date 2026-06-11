using MassTransit;
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

        public RewardsGamificationService(
            IRequestClient<SpendXpRequest> spendXpClient,
            IRequestClient<SetExplorerPremiumRequest> setPremiumClient)
        {
            _spendXpClient = spendXpClient;
            _setPremiumClient = setPremiumClient;
        }

        public async Task<ApiResponse<string>> SpendXpAsync(Guid operationId, Guid explorerId, int amount, string sourceType, Guid referenceId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _spendXpClient.GetResponse<SpendXpResponse>(
                    new SpendXpRequest(operationId, explorerId, amount, sourceType, referenceId),
                    cancellationToken);

                return response.Message.IsSuccess
                    ? ApiResponse<string>.Success(response.Message.Message ?? "XP spent successfully")
                    : ApiResponse<string>.Failure(response.Message.ErrorCode);
            }
            catch (RequestTimeoutException)
            {
                return ApiResponse<string>.Failure(ErrorCode.Timeout);
            }
            catch (Exception)
            {
                return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
            }
        }

        public async Task<ApiResponse<string>> SetPremiumAsync(Guid operationId, Guid explorerId, bool isPremium, Guid? planTierId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _setPremiumClient.GetResponse<SetExplorerPremiumResponse>(
                    new SetExplorerPremiumRequest(operationId, explorerId, isPremium, planTierId),
                    cancellationToken);

                return response.Message.IsSuccess
                    ? ApiResponse<string>.Success(response.Message.Message ?? "Explorer premium state updated successfully")
                    : ApiResponse<string>.Failure(response.Message.ErrorCode);
            }
            catch (RequestTimeoutException)
            {
                return ApiResponse<string>.Failure(ErrorCode.Timeout);
            }
            catch (Exception)
            {
                return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
            }
        }
    }
}
