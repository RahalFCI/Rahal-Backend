using Shared.Application.Interfaces;
using Shared.Domain.Entities;

namespace Users.Application.Interfaces
{
    public interface IUsersRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : BaseEntity
    {
    }
}
