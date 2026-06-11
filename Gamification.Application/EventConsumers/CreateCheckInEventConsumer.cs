using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.CQRS.Orchestrators.UserStat;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Interfaces;
using Gamification.Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Events.CheckIns;

namespace Gamification.Application.EventConsumers
{
    public class CreateCheckInEventConsumer : IConsumer<CreateCheckInEvent>
    {
        private readonly IMediator _mediator;
        private readonly IGamificationUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCheckInEventConsumer> _logger;

        public CreateCheckInEventConsumer(
            IMediator mediator,
            IGamificationUnitOfWork unitOfWork,
            ILogger<CreateCheckInEventConsumer> logger)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CreateCheckInEvent> context)
        {
            var cancellationToken = context.CancellationToken;
            var explorerId = context.Message.ExplorerId;
            var checkInId = context.Message.CheckInId;

            _logger.LogInformation("Processing check-in event for explorer {ExplorerId} with check-in {CheckInId}",
                explorerId, checkInId);

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var xpTransactionResult = await _mediator.Send(
                    new CreateXpTransactionCommand(new CreateXpTransactionDto
                    {
                        ExplorerId = explorerId,
                        ReferenceId = checkInId,
                        SourceType = XpSourceType.CheckIn.ToString()
                    }),
                    cancellationToken);

                if (!xpTransactionResult.IsSuccess)
                {
                    _logger.LogError("Failed to create XP transaction for explorer {ExplorerId}. Error: {ErrorCode}",
                        explorerId, xpTransactionResult.errorCode);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw new Exception($"Failed to create XP transaction: {xpTransactionResult.errorCode}");
                }

                _logger.LogInformation("XP transaction created for explorer {ExplorerId} with check-in {CheckInId}",
                    explorerId, checkInId);

                var checkInStatsResult = await _mediator.Send(
                    new UpdateCheckInStatsOrchestrator(explorerId, checkInId, xpTransactionResult.Data.Amount),
                    cancellationToken);

                if (!checkInStatsResult.IsSuccess)
                {
                    _logger.LogError("Failed to update check-in stats for explorer {ExplorerId}. Error: {ErrorCode}",
                        explorerId, checkInStatsResult.errorCode);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw new Exception($"Failed to process check-in event: {checkInStatsResult.errorCode}");
                }

                _logger.LogInformation("Check-in event processed successfully for explorer {ExplorerId}",
                    explorerId);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing check-in event for explorer {ExplorerId}",
                    explorerId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}

