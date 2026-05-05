using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Domain.Events;

namespace Gamification.Application.EventHandlers
{
    public class ExplorerProfileCreatedEventHandler : INotificationHandler<ExplorerProfileCreatedEvent>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly ILogger<ExplorerProfileCreatedEventHandler> _logger;

        public ExplorerProfileCreatedEventHandler(
            IGenericRepository<ExplorerProfile> repository,
            ILogger<ExplorerProfileCreatedEventHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(ExplorerProfileCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating explorer profile for user {UserId}", notification.UserId);

            var explorerProfile = new ExplorerProfile
            {
                UserId = notification.UserId,
                Gender = notification.Gender,
                BirthDate = notification.BirthDate,
                Bio = notification.Bio,
                CountryCode = notification.CountryCode
            };

            _repository.Add(explorerProfile);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Explorer profile created for user {UserId}", notification.UserId);
        }
    }
}
