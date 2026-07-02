using Gamification.Application.CQRS.Queries.UserStats;
using Gamification.Application.DTOs.UserStats;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.UserStat.Queries
{
    public class GetUserStatsByExplorerIdQueryHandler : IRequestHandler<GetUserStatsByExplorerIdQuery, ApiResponse<GetUserStatsDto>>
    {
        private readonly IGamificationRepository<UserStats> _repository;
        private readonly ILogger<GetUserStatsByExplorerIdQueryHandler> _logger;

        public GetUserStatsByExplorerIdQueryHandler(
            IGamificationRepository<UserStats> repository,
            ILogger<GetUserStatsByExplorerIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetUserStatsDto>> Handle(GetUserStatsByExplorerIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching user stats for explorer {ExplorerId}", request.ExplorerId);

            var userStats = await _repository.GetTable()
                .Include(s => s.ExplorerProfile)
                .Where(s => s.ExplorerProfileId == request.ExplorerId)
                .FirstOrDefaultAsync(cancellationToken);

            if (userStats is null)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                return ApiResponse<GetUserStatsDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<GetUserStatsDto>.Success(UserStatsMapper.ToGetDto(userStats));
        }
    }
}
