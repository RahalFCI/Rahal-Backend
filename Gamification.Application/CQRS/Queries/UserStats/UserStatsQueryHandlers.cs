using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.DTOs.UserStats;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamification.Application.CQRS.Queries.UserStat
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

    public class GetAllUserStatsQueryHandler : IRequestHandler<GetAllUserStatsQuery, IEnumerable<GetUserStatsDto>>
    {
        private readonly IGenericRepository<UserStats> _repository;
        private readonly ILogger<GetAllUserStatsQueryHandler> _logger;

        public GetAllUserStatsQueryHandler(
            IGenericRepository<UserStats> repository,
            ILogger<GetAllUserStatsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<GetUserStatsDto>> Handle(GetAllUserStatsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all user stats");

            var statsList = await _repository.GetAllAsync(cancellationToken: cancellationToken);
            var dtos = UserStatsMapper.ToGetDtos(statsList);

            _logger.LogInformation("Retrieved {Count} user stats records", statsList.Count());

            return dtos;
        }
    }
}
