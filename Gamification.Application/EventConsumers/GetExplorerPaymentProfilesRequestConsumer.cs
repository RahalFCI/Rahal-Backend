using Gamification.Application.Interfaces;
using Gamification.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Events.Gamification;
using Shared.Domain.Enums;

namespace Gamification.Application.EventConsumers
{
    public class GetExplorerPaymentProfilesRequestConsumer
        : IConsumer<GetExplorerPaymentProfilesRequest>
    {
        private readonly IGamificationRepository<ExplorerProfile> _explorerRepository;
        private readonly ILogger<GetExplorerPaymentProfilesRequestConsumer> _logger;

        public GetExplorerPaymentProfilesRequestConsumer(
            IGamificationRepository<ExplorerProfile> explorerRepository,
            ILogger<GetExplorerPaymentProfilesRequestConsumer> logger)
        {
            _explorerRepository = explorerRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<GetExplorerPaymentProfilesRequest> context)
        {
            var request = context.Message;

            try
            {
                var query = _explorerRepository
                    .GetTable()
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.DisplayName))
                {
                    var displayName = request.DisplayName.Trim().ToLower();
                    query = query.Where(explorer =>
                        explorer.DisplayName.ToLower().Contains(displayName));
                }

                if (request.ExplorerIds is { Count: > 0 })
                {
                    var explorerIds = request.ExplorerIds.Distinct().ToArray();
                    query = query.Where(explorer => explorerIds.Contains(explorer.UserId));
                }

                var explorers = await query
                    .Select(explorer => new ExplorerPaymentProfileDto(
                        explorer.UserId,
                        explorer.DisplayName))
                    .ToListAsync(context.CancellationToken);

                await context.RespondAsync(new GetExplorerPaymentProfilesResponse(
                    true,
                    ErrorCode.None,
                    explorers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve explorer payment profiles.");

                await context.RespondAsync(new GetExplorerPaymentProfilesResponse(
                    false,
                    ErrorCode.UnknownError,
                    Array.Empty<ExplorerPaymentProfileDto>()));
            }
        }
    }
}
