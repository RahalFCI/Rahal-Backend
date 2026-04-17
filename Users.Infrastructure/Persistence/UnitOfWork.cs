using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.Interfaces;

namespace Users.Infrastructure.Persistence
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly UsersDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public bool HasActiveTransaction => _transaction != null;

        public UnitOfWork(UsersDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));

            if (_transaction != null)
            {
                _logger.LogWarning("A transaction is already active. Skipping BeginTransaction.");
                return;
            }

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _logger.LogInformation("Transaction {TransactionId} started.", _transaction.TransactionId);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));

            if (_transaction == null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Transaction {TransactionId} committed.", _transaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction {TransactionId} failed. Rolling back.", _transaction.TransactionId);
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                _logger.LogWarning("No active transaction to roll back.");
                return;
            }

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
                _logger.LogInformation("Transaction {TransactionId} rolled back.", _transaction.TransactionId);
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));

            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict detected during SaveChanges.");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during SaveChanges.");
                throw;
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            if (_transaction != null)
            {
                _logger.LogWarning("UnitOfWork disposed with an active transaction. Rolling back.");
                await RollbackTransactionAsync();
            }

            await _context.DisposeAsync();
            _disposed = true;
        }
    }
}
