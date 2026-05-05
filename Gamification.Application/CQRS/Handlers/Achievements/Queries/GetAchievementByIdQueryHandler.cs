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
    public class GetAchievementByIdQueryHandler : IRequestHandler<GetAchievementByIdQuery, GetAchievementDto?>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly ILogger<GetAchievementByIdQueryHandler> _logger;

        public GetAchievementByIdQueryHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            ILogger<GetAchievementByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetAchievementDto?> Handle(GetAchievementByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching achievement {AchievementId}", request.Id);

            var achievement = await _repository.GetTable()
                .Where(a => request.Id == a.Id)
                .Include(a => a.Badge)
                .Include(a => a.AchievementCriteriaType)
                .FirstOrDefaultAsync(cancellationToken);

            if (achievement is null)
            {
                _logger.LogWarning("Achievement {AchievementId} not found", request.Id);
                return null;
            }

            return AchievementMapper.ToGetDto(achievement);
        }
    }
}
