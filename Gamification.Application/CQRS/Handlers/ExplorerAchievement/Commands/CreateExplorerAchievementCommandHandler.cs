using Gamification.Application.CQRS.Commands.ExplorerAchievement;
using Gamification.Application.CQRS.Queries.Achievement;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.ExplorerAchievement.Commands
{
    public class CreateExplorerAchievementCommandHandler : IRequestHandler<CreateExplorerAchievementCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.ExplorerAchievement> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateExplorerAchievementCommandHandler> _logger;

        public CreateExplorerAchievementCommandHandler(
            IGenericRepository<Domain.Entities.ExplorerAchievement> repository,
            IMediator mediator,
            ILogger<CreateExplorerAchievementCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(CreateExplorerAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating explorer achievement for explorer {ExplorerId} and achievement {AchievementId}",
                request.Dto.ExplorerId, request.Dto.AchievementId);

            var achievement = await _mediator.Send(new GetAchievementByIdQuery(request.Dto.AchievementId), cancellationToken);

            if (achievement is null)
            {
                _logger.LogWarning("Achievement {AchievementId} not found", request.Dto.AchievementId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var explorerAchievementExists = await _repository.GetTable()
                .AnyAsync(ea => ea.AchievementId == request.Dto.AchievementId && ea.ExplorerId == request.Dto.ExplorerId,
                    cancellationToken);

            if (explorerAchievementExists)
            {
                _logger.LogWarning("Explorer {ExplorerId} already has achievement {AchievementId}",
                    request.Dto.ExplorerId, request.Dto.AchievementId);
                return ApiResponse<string>.Failure(ErrorCode.Conflict);
            }

            var explorerAchievement = ExplorerAchievementMapper.ToEntity(request.Dto);
            _repository.Add(explorerAchievement);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Explorer achievement created with ID {ExplorerAchievementId}", explorerAchievement.Id);

            return ApiResponse<string>.Success($"Explorer achievement created successfully. ID: {explorerAchievement.Id}");
        }
    }
}
