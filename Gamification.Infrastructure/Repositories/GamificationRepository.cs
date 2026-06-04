using Gamification.Infrastructure.Persistence;
using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;

namespace Gamification.Infrastructure.Repositories
{
    public class GamificationRepository<TEntity> : GenericRepository<TEntity, GamificationDbContext>
        where TEntity : BaseEntity
    {
        public GamificationRepository(GamificationDbContext context)
            : base(context)
        {
        }
    }
}
