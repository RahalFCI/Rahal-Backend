using Gamification.Application.CQRS.Queries.Achievement;
using Gamification.Application.DTOs.Achievement;
using Gamification.Application.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Achievements.Queries
{
    public class GetAchievementsByBadgeIdQueryHandler : IRequestHandler<GetAchievementsByBadgeIdQuery, IEnumerable<GetAchievementDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly ILogger<GetAchievementsByBadgeIdQueryHandler> _logger;

        public GetAchievementsByBadgeIdQueryHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            ILogger<GetAchievementsByBadgeIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<GetAchievementDto>> Handle(GetAchievementsByBadgeIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching achievements for badge {BadgeId}", request.BadgeId);

            var achievements = await _repository.GetTable()
                .Where(a => a.BadgeId == request.BadgeId)
                .Include(a => a.Badge)
                .Include(a => a.AchievementCriteriaType)
                .ToListAsync(cancellationToken);

            var dtos = AchievementMapper.ToGetDtos(achievements);

            _logger.LogInformation("Retrieved {Count} achievements for badge {BadgeId}", achievements.Count(), request.BadgeId);

            return dtos;
        }
    }
}
