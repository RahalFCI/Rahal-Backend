using Gamification.Application.CQRS.Commands.Badges;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Badges.Commands
{
    public class DeleteBadgeCommandHandler : IRequestHandler<DeleteBadgeCommand, string>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly ILogger<DeleteBadgeCommandHandler> _logger;

        public DeleteBadgeCommandHandler(
            IGenericRepository<Badge> repository,
            ILogger<DeleteBadgeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(DeleteBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting badge {BadgeId}", request.Id);

            var badge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (badge is null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Id);
                return $"Badge not found";
            }

            _repository.Delete(badge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Badge {BadgeId} deleted successfully", request.Id);

            return "Badge deleted successfully";
        }
    }
}
