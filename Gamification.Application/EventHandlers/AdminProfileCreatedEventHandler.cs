using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Domain.Events;

namespace Gamification.Application.EventHandlers
{
    public class AdminProfileCreatedEventHandler : INotificationHandler<AdminProfileCreatedEvent>
    {
        private readonly IGenericRepository<AdminProfile> _repository;
        private readonly ILogger<AdminProfileCreatedEventHandler> _logger;

        public AdminProfileCreatedEventHandler(
            IGenericRepository<AdminProfile> repository,
            ILogger<AdminProfileCreatedEventHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(AdminProfileCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating admin profile for user {UserId}", notification.UserId);

            var adminProfile = new AdminProfile
            {
                UserId = notification.UserId
            };

            _repository.Add(adminProfile);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Admin profile created for user {UserId}", notification.UserId);
        }
    }
}
