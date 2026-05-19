using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.UserStat.Commands
{
    internal class RebuildLeaderboardCommandHandler : IRequestHandler<RebuildLeaderboardCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<UserStats> _repository;
        private readonly ICacheService _cacheService;

        public RebuildLeaderboardCommandHandler(IGenericRepository<UserStats> repository, ICacheService cacheService)
        {
            _repository = repository;
            _cacheService = cacheService;
        }

        public async Task<ApiResponse<string>> Handle(RebuildLeaderboardCommand request, CancellationToken cancellationToken)
        {
            await _cacheService.RemoveAsync("leaderboard:xp");

            var topUsers = await _repository.GetTable()
                .OrderByDescending(us => us.CumulativeXp)
                .Select(us => new { us.ExplorerProfileId, us.CumulativeXp })
                .Take(10)
                .ToListAsync();

            foreach (var user in topUsers)
                await _cacheService.SortedSetAddAsync("leaderboard:xp", user.ExplorerProfileId.ToString(), user.CumulativeXp);

            return ApiResponse<string>.Success("Leaderboard is up to date");
        }
    }
}
