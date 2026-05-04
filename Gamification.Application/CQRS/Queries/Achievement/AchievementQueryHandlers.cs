using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.DTOs.Achievement;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamification.Application.CQRS.Queries.Achievement
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
