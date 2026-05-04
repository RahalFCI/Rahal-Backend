using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.DTOs.Badge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;

namespace Gamification.Application.CQRS.Queries.Badges
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

    public class GetAllBadgesQueryHandler : IRequestHandler<GetAllBadgesQuery, IEnumerable<GetBadgeDto>>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly ILogger<GetAllBadgesQueryHandler> _logger;

        public GetAllBadgesQueryHandler(
            IGenericRepository<Badge> repository,
            ILogger<GetAllBadgesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<GetBadgeDto>> Handle(GetAllBadgesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all badges");

            var badges = await _repository.GetAllAsync(cancellationToken: cancellationToken);
            var dtos = BadgeMapper.ToGetDtos(badges);

            _logger.LogInformation("Retrieved {Count} badges", badges.Count());

            return dtos;
        }
    }
}
