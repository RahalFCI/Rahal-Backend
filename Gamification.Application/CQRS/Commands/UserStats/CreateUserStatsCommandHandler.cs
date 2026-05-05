using Gamification.Application.CQRS.Handlers.UserStat.Commands;
using Gamification.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.UserStats
{
    public class CreateUserStatsCommandHandler : IRequestHandler<CreateUserStatsCommand, string>
    {
        private readonly IGenericRepository<Domain.Entities.UserStats> _repository;
        private readonly ILogger<CreateUserStatsCommandHandler> _logger;

        public CreateUserStatsCommandHandler(
            IGenericRepository<Domain.Entities.UserStats> repository,
            ILogger<CreateUserStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(CreateUserStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating user stats for explorer {ExplorerId}", request.Dto.ExplorerId);

            var userStats = UserStatsMapper.ToEntity(request.Dto);
            _repository.Add(userStats);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User stats {UserStatsId} created successfully for explorer {ExplorerId}",
                userStats.Id, request.Dto.ExplorerId);

            return $"User stats created successfully. ID: {userStats.Id}";
        }
    }
}
