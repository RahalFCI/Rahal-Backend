using Payment.Infrastructure.Persistence;
using Shared.Domain.Entities;
using Shared.Infrastructure.Repositories;

namespace Payment.Infrastructure.Repositories
{
    public class PaymentRepository<TEntity> : GenericRepository<TEntity, PaymentDbContext>
        where TEntity : BaseEntity
    {
        public PaymentRepository(PaymentDbContext context)
            : base(context)
        {
        }
    }
}
