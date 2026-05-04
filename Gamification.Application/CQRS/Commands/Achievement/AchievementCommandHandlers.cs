using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.DTOs.Achievement;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;

namespace Gamification.Application.CQRS.Commands.Achievement
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

    public class UpdateAchievementCommandHandler : IRequestHandler<UpdateAchievementCommand, string>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly IGenericRepository<Domain.Entities.Badge> _badgeRepository;
        private readonly IGenericRepository<Domain.Entities.AchievementCriteriaType> _criteriaTypeRepository;
        private readonly ILogger<UpdateAchievementCommandHandler> _logger;

        public UpdateAchievementCommandHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            IGenericRepository<Domain.Entities.Badge> badgeRepository,
            IGenericRepository<Domain.Entities.AchievementCriteriaType> criteriaTypeRepository,
            ILogger<UpdateAchievementCommandHandler> logger)
        {
            _repository = repository;
            _badgeRepository = badgeRepository;
            _criteriaTypeRepository = criteriaTypeRepository;
            _logger = logger;
        }

        public async Task<string> Handle(UpdateAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating achievement {AchievementId}", request.Id);

            var achievement = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (achievement is null)
            {
                _logger.LogWarning("Achievement {AchievementId} not found", request.Id);
                return "Achievement not found";
            }

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

            AchievementMapper.UpdateEntity(achievement, request.Dto);
            _repository.Update(achievement);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Achievement {AchievementId} updated successfully", request.Id);

            return "Achievement updated successfully";
        }
    }

    public class DeleteAchievementCommandHandler : IRequestHandler<DeleteAchievementCommand, string>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly ILogger<DeleteAchievementCommandHandler> _logger;

        public DeleteAchievementCommandHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            ILogger<DeleteAchievementCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(DeleteAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting achievement {AchievementId}", request.Id);

            var achievement = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (achievement is null)
            {
                _logger.LogWarning("Achievement {AchievementId} not found", request.Id);
                return "Achievement not found";
            }

            _repository.Delete(achievement);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Achievement {AchievementId} deleted successfully", request.Id);

            return "Achievement deleted successfully";
        }
    }
}
