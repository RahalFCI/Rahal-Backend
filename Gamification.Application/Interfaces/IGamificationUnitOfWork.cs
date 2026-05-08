using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Interfaces
{
    public interface IGamificationUnitOfWork
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
    }
}
