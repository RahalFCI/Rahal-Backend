using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.CQRS.Queries.UserStats;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Interfaces;
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

namespace Gamification.Application.CQRS.Handlers.XpTransactions.Commands
{
    internal class CreateCustomXpTransactionCommandHandler : IRequestHandler<CreateCustomXpTransactionCommand, ApiResponse<GetXpTransactionDto>>
    {
        private readonly IGamificationRepository<Domain.Entities.XpTransaction> _repository;
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CreateCustomXpTransactionCommandHandler> _logger;

        public CreateCustomXpTransactionCommandHandler(
            IGamificationRepository<XpTransaction> repository,
            IMediator mediator,
            ICacheService cacheService,
            ILogger<CreateCustomXpTransactionCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _cacheService = cacheService;
            _logger = logger;
        }
        public async Task<ApiResponse<GetXpTransactionDto>> Handle(CreateCustomXpTransactionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating XP transaction for explorer {ExplorerId}", request.ExplorerId);

            var existingTransaction = await _repository.GetTable()
                .FirstOrDefaultAsync(x =>
                    x.ExplorerProfileId == request.ExplorerId &&
                    x.Source == Enum.Parse<XpSourceType>(request.SourceType) &&
                    x.ReferenceId == request.ReferenceId,
                    cancellationToken);

            if (existingTransaction is not null)
            {
                return ApiResponse<GetXpTransactionDto>.Failure(ErrorCode.AlreadyExists);
            }

            var user = await _mediator.Send(new GetExplorerProfileByIdQuery(request.ExplorerId), cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Explorer {ExplorerId} not found", request.ExplorerId);
                return ApiResponse<GetXpTransactionDto>.Failure(ErrorCode.NotFound);
            }

            var sourceType = Enum.Parse<XpSourceType>(request.SourceType);

            int xpAmount = request.XpAmount;

            if (user.Data.IsPremium)
            {
                xpAmount = (int)(xpAmount * 1.5);
            }

            var transaction = new XpTransaction
            {
                ExplorerProfileId = request.ExplorerId,
                Amount = xpAmount,
                Source = sourceType,
                ReferenceId = request.ReferenceId
            };

            _repository.Add(transaction);
            await _repository.SaveChangesAsync(cancellationToken);

            // Update cache
            var userStats = await _mediator.Send(new GetUserStatsByExplorerIdQuery(request.ExplorerId), cancellationToken);
            await _cacheService.SortedSetAddAsync("leaderboard:xp", request.ExplorerId.ToString(), userStats.Data.CumulativeXp + xpAmount);

            _logger.LogInformation("XP transaction {TransactionId} created with {Amount} XP for explorer {ExplorerId}",
                transaction.Id, xpAmount, request.ExplorerId);

            return ApiResponse<GetXpTransactionDto>.Success(new GetXpTransactionDto
            {
                Id = transaction.Id,
                ExplorerId = transaction.ExplorerProfileId,
                Amount = transaction.Amount,
                SourceType = transaction.Source.ToString(),
                ReferenceId = transaction.ReferenceId
            });
        }
    }
}
