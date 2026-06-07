using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence
{
    internal class GamificationDbInitializer : IDbInitializer
    {
        private readonly GamificationDbContext _context;

        public GamificationDbInitializer(GamificationDbContext context)
        {
            _context = context;
        }
        public async Task SeedAsync()
        {
            if (await _context.AchievementCriteriaType.AnyAsync())
                return;

            var criteriaTypes = new List<AchievementCriteriaType>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Total Check-ins",
                Description = "Total number of verified check-ins",
                Code = "TOTAL_CHECKINS"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Total XP",
                Description = "Total cumulative XP earned",
                Code = "TOTAL_XP"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Longest Streak",
                Description = "Longest consecutive daily activity streak",
                Code = "LONGEST_STREAK"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Total Challenges",
                Description = "Total number of completed challenges",
                Code = "TOTAL_CHALLENGES"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Total Badges",
                Description = "Total number of badges earned",
                Code = "TOTAL_BADGES"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Total Achievements",
                Description = "Total number of achievements earned",
                Code = "TOTAL_ACHIEVEMENTS"
            }
        };

            await _context.AchievementCriteriaType.AddRangeAsync(criteriaTypes);
            await _context.SaveChangesAsync();
        }
    }
}
