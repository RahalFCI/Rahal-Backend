using Shared.Application.Interfaces;
using Shared.Domain.Entities;

namespace Rewards.Application.Interfaces
{
    public interface IRewardsRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : BaseEntity
    {
    }
}
