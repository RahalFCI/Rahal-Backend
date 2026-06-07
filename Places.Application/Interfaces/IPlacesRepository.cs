using Shared.Application.Interfaces;
using Shared.Domain.Entities;

namespace Places.Application.Interfaces
{
    public interface IPlacesRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : BaseEntity
    {
    }
}
