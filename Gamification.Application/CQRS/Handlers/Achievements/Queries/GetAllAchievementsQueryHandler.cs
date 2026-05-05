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
    public class GetAllAchievementsQueryHandler : IRequestHandler<GetAllAchievementsQuery, IEnumerable<GetAchievementDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly ILogger<GetAllAchievementsQueryHandler> _logger;

        public GetAllAchievementsQueryHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            ILogger<GetAllAchievementsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<GetAchievementDto>> Handle(GetAllAchievementsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all achievements");

            var achievements = await _repository.GetTable()
                .Include(a => a.Badge)
                .Include(a => a.AchievementCriteriaType)
                .ToListAsync(cancellationToken);

            var dtos = AchievementMapper.ToGetDtos(achievements);

            _logger.LogInformation("Retrieved {Count} achievements", achievements.Count());

            return dtos;
        }
    }
}
