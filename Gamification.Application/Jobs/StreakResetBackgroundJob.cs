using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;

namespace Gamification.Application.Jobs
{
    public class StreakResetBackgroundJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StreakResetBackgroundJob> _logger;

        public StreakResetBackgroundJob(IServiceScopeFactory scopeFactory, ILogger<StreakResetBackgroundJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserStats>>();

            var cutoffDate = DateTime.UtcNow.AddDays(-1);

            var resetCount = await repository.GetTable()
                .Where(us => us.LastActivityDate < cutoffDate && us.CurrentStreak > 0)
                .ExecuteUpdateAsync(s => s.SetProperty(us => us.CurrentStreak, 0), cancellationToken);

            _logger.LogInformation("Streak reset job completed. Reset {Count} explorers", resetCount);
        }
    }
}
