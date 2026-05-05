using Gamification.Application.CQRS.Commands.Achievement;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Achievements.Commands
{
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
