using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Domain.Events;

namespace Gamification.Application.EventHandlers
{
    public class VendorProfileCreatedEventHandler : INotificationHandler<VendorProfileCreatedEvent>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<VendorProfileCreatedEventHandler> _logger;

        public VendorProfileCreatedEventHandler(
            IGenericRepository<VendorProfile> repository,
            ILogger<VendorProfileCreatedEventHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(VendorProfileCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating vendor profile for user {UserId}", notification.UserId);

            var vendorProfile = new VendorProfile
            {
                UserId = notification.UserId,
                CountryCode = notification.CountryCode,
                Address = notification.Address,
                AddressUrl = notification.AddressUrl,
                WorkingHours = notification.WorkingHours,
                CategoryId = notification.CategoryId
            };

            _repository.Add(vendorProfile);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Vendor profile created for user {UserId}", notification.UserId);
        }
    }
}
