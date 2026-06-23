using System;
using System.Collections.Generic;
using System.Text;

namespace Rewards.Application.Interfaces
{
    public interface IRewardsUnitOfWork
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
    }
}
