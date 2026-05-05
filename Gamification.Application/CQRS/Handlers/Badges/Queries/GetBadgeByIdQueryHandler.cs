using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.DTOs.Badge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Badges.Queries
{
    public class GetBadgeByIdQueryHandler : IRequestHandler<GetBadgeByIdQuery, GetBadgeDto?>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly ILogger<GetBadgeByIdQueryHandler> _logger;

        public GetBadgeByIdQueryHandler(
            IGenericRepository<Badge> repository,
            ILogger<GetBadgeByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetBadgeDto?> Handle(GetBadgeByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching badge {BadgeId}", request.Id);

            var badge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (badge is null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Id);
                return null;
            }

            return BadgeMapper.ToGetDto(badge);
        }
    }
}
