using Gamification.Application.CQRS.Commands.UserStat;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Events.Payments;

namespace Gamification.Application.EventConsumers
{
    public class SpendXpRequestConsumer : IConsumer<SpendXpRequest>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SpendXpRequestConsumer> _logger;

        public SpendXpRequestConsumer(IMediator mediator, ILogger<SpendXpRequestConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SpendXpRequest> context)
        {
            var request = context.Message;
            var result = await _mediator.Send(
                new SpendXpCommand(request.ExplorerId, request.Amount, request.SourceType, request.ReferenceId),
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
                result.IsSuccess ? result.Data : null));
        }
    }
}
