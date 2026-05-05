using Gamification.Application.CQRS.Commands.Badges;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Badges.Commands
{
    public class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommand, string>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly ILogger<UpdateBadgeCommandHandler> _logger;

        public UpdateBadgeCommandHandler(
            IGenericRepository<Badge> repository,
            ILogger<UpdateBadgeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(UpdateBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating badge {BadgeId}", request.Id);

            var badge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (badge is null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Id);
                return $"Badge not found";
            }

            BadgeMapper.UpdateEntity(badge, request.Dto);
            _repository.Update(badge);
            await _repository.SaveChangesAsync(cancellationToken);


            _logger.LogInformation("Badge {BadgeId} updated successfully", request.Id);

            return "Badge updated successfully";
        }
    }
}
