using Users.Infrastructure.Persistence;
using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;
using Users.Application.Interfaces;

namespace Users.Infrastructure.Repositories
{
    public class UsersRepository<TEntity> : GenericRepository<TEntity, UsersDbContext>, IUsersRepository<TEntity>
        where TEntity : BaseEntity
    {
        public UsersRepository(UsersDbContext context)
            : base(context)
        {
        }
    }
}
