using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Events.Payments;

namespace Gamification.Application.EventConsumers
{
    public class SetExplorerPremiumRequestConsumer : IConsumer<SetExplorerPremiumRequest>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SetExplorerPremiumRequestConsumer> _logger;

        public SetExplorerPremiumRequestConsumer(IMediator mediator, ILogger<SetExplorerPremiumRequestConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SetExplorerPremiumRequest> context)
        {
            var request = context.Message;
            var result = await _mediator.Send(
                new SetExplorerPremiumCommand(request.ExplorerId, request.IsPremium, request.PlanTierId),
                context.CancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "Set premium request {OperationId} failed for explorer {ExplorerId}. Error: {ErrorCode}",
                    request.OperationId,
                    request.ExplorerId,
                    result.errorCode);
            }

            await context.RespondAsync(new SetExplorerPremiumResponse(
                request.OperationId,
                result.IsSuccess,
                result.errorCode,
                result.IsSuccess ? result.Data : null));
        }
    }
}
