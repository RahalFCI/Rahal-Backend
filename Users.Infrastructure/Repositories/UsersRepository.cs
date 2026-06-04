using Users.Infrastructure.Persistence;
using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;

namespace Users.Infrastructure.Repositories
{
    public class UsersRepository<TEntity> : GenericRepository<TEntity, UsersDbContext>
        where TEntity : BaseEntity
    {
        public UsersRepository(UsersDbContext context)
            : base(context)
        {
        }
    }
}
