using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.CQRS.Handlers.XpTransactions.Commands;
using Gamification.Application.Strategies;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.XpTransactions.Commands
{
    public class DeleteXpTransactionCommandHandler : IRequestHandler<DeleteXpTransactionCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<XpTransaction> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<DeleteXpTransactionCommandHandler> _logger;

        public DeleteXpTransactionCommandHandler(
            IGamificationRepository<XpTransaction> repository,
            ICacheService cacheService,
            ILogger<DeleteXpTransactionCommandHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteXpTransactionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting XP transaction {TransactionId}", request.Id);

            var transaction = await _repository.GetTable().Where(t => t.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
            if (transaction is null)
            {
                _logger.LogWarning("XP transaction {TransactionId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            transaction.IsDeleted = true;
            transaction.DeletedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync(cancellationToken);

            // Update leaderboard cache for the user
            var newXp = request.ExistingXp - transaction.Amount;
            await _cacheService.SortedSetAddAsync("leaderboard:xp", request.ExplorerId.ToString(), newXp);

            _logger.LogInformation("XP transaction {TransactionId} deleted", request.Id);
            return ApiResponse<string>.Success("XP transaction deleted successfully");
        }
    }
}
            