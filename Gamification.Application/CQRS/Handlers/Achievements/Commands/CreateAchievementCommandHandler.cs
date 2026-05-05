using Gamification.Application.CQRS.Commands.Achievement;
using Gamification.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Achievements.Commands
{
    public class CreateAchievementCommandHandler : IRequestHandler<CreateAchievementCommand, string>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly IGenericRepository<Domain.Entities.Badge> _badgeRepository;
        private readonly IGenericRepository<Domain.Entities.AchievementCriteriaType> _criteriaTypeRepository;
        private readonly ILogger<CreateAchievementCommandHandler> _logger;

        public CreateAchievementCommandHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            IGenericRepository<Domain.Entities.Badge> badgeRepository,
            IGenericRepository<Domain.Entities.AchievementCriteriaType> criteriaTypeRepository,
            ILogger<CreateAchievementCommandHandler> logger)
        {
            _repository = repository;
            _badgeRepository = badgeRepository;
            _criteriaTypeRepository = criteriaTypeRepository;
            _logger = logger;
        }

        public async Task<string> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating achievement {AchievementTitle}", request.Dto.Title);

            var badge = await _badgeRepository.GetByIdAsync(request.Dto.BadgeId, cancellationToken);
            if (badge is null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Dto.BadgeId);
                return "Badge not found";
            }

            var criteriaType = await _criteriaTypeRepository.GetByIdAsync(request.Dto.CriteriaTypeId, cancellationToken);
            if (criteriaType is null)
            {
                _logger.LogWarning("Criteria type {CriteriaTypeId} not found", request.Dto.CriteriaTypeId);
                return "Criteria type not found";
            }

            var achievement = AchievementMapper.ToEntity(request.Dto);
            _repository.Add(achievement);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Achievement {AchievementId} created successfully", achievement.Id);

            return $"Achievement created successfully. ID: {achievement.Id}";
        }
    }
}
