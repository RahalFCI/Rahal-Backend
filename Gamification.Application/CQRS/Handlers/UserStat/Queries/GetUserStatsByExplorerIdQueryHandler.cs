using Gamification.Application.CQRS.Queries.UserStats;
using Gamification.Application.DTOs.UserStats;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.UserStat.Queries
{
    public class GetUserStatsByExplorerIdQueryHandler : IRequestHandler<GetUserStatsByExplorerIdQuery, GetUserStatsDto?>
    {
        private readonly IGenericRepository<UserStats> _repository;
        private readonly ILogger<GetUserStatsByExplorerIdQueryHandler> _logger;

        public GetUserStatsByExplorerIdQueryHandler(
            IGenericRepository<UserStats> repository,
            ILogger<GetUserStatsByExplorerIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetUserStatsDto?> Handle(GetUserStatsByExplorerIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching user stats for explorer {ExplorerId}", request.ExplorerId);

            var userStats = await _repository.GetTable()
                .Where(s => s.ExplorerProfileId == request.ExplorerId)
                .FirstOrDefaultAsync(cancellationToken);

            if (userStats is null)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                return null;
            }

            return UserStatsMapper.ToGetDto(userStats);
        }
    }
}
