using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.CQRS.Queries.UserStats;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Events.Payments;
using Shared.Application.Interfaces;

namespace Gamification.Application.EventConsumers
{
    public class SpendXpRequestConsumer : IConsumer<SpendXpRequest>
    {
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SpendXpRequestConsumer> _logger;

        public SpendXpRequestConsumer(IMediator mediator, ICacheService cacheService, ILogger<SpendXpRequestConsumer> logger)
        {
            _mediator = mediator;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SpendXpRequest> context)
        {
            var request = context.Message;
            var result = await _mediator.Send(
                new CreateXpTransactionCommand(new CreateXpTransactionDto {ExplorerId = request.ExplorerId, SourceType =  XpSourceType.Payment.ToString(), ReferenceId = request.ReferenceId }),
                context.CancellationToken);


            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "Spend XP request {OperationId} failed for explorer {ExplorerId}. Error: {ErrorCode}",
                    request.OperationId,
                    request.ExplorerId,
                    result.errorCode);
            }

            await context.RespondAsync(new SpendXpResponse(
                request.OperationId,
                result.IsSuccess,
                result.errorCode,
                result.IsSuccess ? "Xp transaction completed successfully" : null));
        }
    }
}
