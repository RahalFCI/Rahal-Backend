using Rewards.Application.Interfaces;
using Rewards.Infrastructure.Persistence;
using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;


namespace Rewards.Infrastructure.Repositories
{
    public class RewardsRepository<TEntity> : GenericRepository<TEntity, RewardsDbContext>, IRewardsRepository<TEntity>
        where TEntity : BaseEntity
    {
        public RewardsRepository(RewardsDbContext context)
            : base(context)
        {
        }
    }
}
