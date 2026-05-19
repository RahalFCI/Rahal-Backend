using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.XpTransactions.Commands
{
    public class RestoreDeletedXpTransactionCommandHandler : IRequestHandler<RestoreDeletedXpTransactionCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<XpTransaction> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RestoreDeletedXpTransactionCommandHandler> _logger;

        public RestoreDeletedXpTransactionCommandHandler(
            IGenericRepository<XpTransaction> repository,
            ICacheService cacheService,
            ILogger<RestoreDeletedXpTransactionCommandHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedXpTransactionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring deleted XP transaction {XpTransactionId}", request.Id);

            var xpTransactionExists = await _repository.GetTable()
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id == request.Id && x.IsDeleted, cancellationToken);

            if (!xpTransactionExists)
            {
                _logger.LogWarning("Deleted XP transaction {XpTransactionId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            XpTransaction xpTransaction = new XpTransaction()
            {
                Id = request.Id,
                IsDeleted = false,
                DeletedAt = null
            };

            _repository.SaveInclude(xpTransaction, nameof(xpTransaction.IsDeleted), nameof(xpTransaction.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            // Update leaderboard cache for the user
            var newXp = request.ExistingXp + xpTransaction.Amount;
            await _cacheService.SortedSetAddAsync("leaderboard:xp", request.ExplorerId.ToString(), newXp);

            _logger.LogInformation("XP transaction {XpTransactionId} restored successfully", request.Id);

            return ApiResponse<string>.Success("XP transaction restored successfully");
        }
    }
}
