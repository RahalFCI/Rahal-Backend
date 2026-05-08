using Gamification.Application.CQRS.Queries.UserStats;
using Gamification.Application.DTOs.UserStats;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.UserStat.Queries
{
    public class GetAllUserStatsQueryHandler : IRequestHandler<GetAllUserStatsQuery, ApiResponse<IEnumerable<GetUserStatsDto>>>
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

        public async Task<ApiResponse<IEnumerable<GetUserStatsDto>>> Handle(GetAllUserStatsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all user stats");

            var statsList = await _repository.GetAllAsync(cancellationToken: cancellationToken);
            var dtos = UserStatsMapper.ToGetDtos(statsList);

            _logger.LogInformation("Retrieved {Count} user stats records", statsList.Count());

            return ApiResponse<IEnumerable<GetUserStatsDto>>.Success(dtos);
        }
    }
}
