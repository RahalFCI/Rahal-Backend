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
    public class CreateXpTransactionCommandHandler : IRequestHandler<CreateXpTransactionCommand, ApiResponse<GetXpTransactionDto>>
    {
        private readonly IGamificationRepository<Domain.Entities.XpTransaction> _repository;
        private readonly IMediator _mediator;
        private readonly XpCalculationStrategyResolver _strategyResolver;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CreateXpTransactionCommandHandler> _logger;

        public CreateXpTransactionCommandHandler(
            IGamificationRepository<XpTransaction> repository,
            IMediator mediator,
            XpCalculationStrategyResolver strategyResolver,
            ICacheService cacheService,
            ILogger<CreateXpTransactionCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _strategyResolver = strategyResolver;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<GetXpTransactionDto>> Handle(CreateXpTransactionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating XP transaction for explorer {ExplorerId}", request.Dto.ExplorerId);

            var existingTransaction = await _repository.GetTable()
            .FirstOrDefaultAsync(x =>
                x.ExplorerProfileId == request.Dto.ExplorerId &&
                x.Source == Enum.Parse<XpSourceType>(request.Dto.SourceType) &&
                x.ReferenceId == request.Dto.ReferenceId,
                cancellationToken);

            if (existingTransaction is not null)
            {
                return ApiResponse<GetXpTransactionDto>.Failure(ErrorCode.AlreadyExists);
            }

            var user = await _mediator.Send(new GetExplorerProfileByIdQuery(request.Dto.ExplorerId ), cancellationToken);
            if(user == null)
            {
                _logger.LogWarning("Explorer {ExplorerId} not found", request.Dto.ExplorerId);
                return ApiResponse<GetXpTransactionDto>.Failure(ErrorCode.NotFound);
            }

            var sourceType = Enum.Parse<XpSourceType>(request.Dto.SourceType);
            var strategy = _strategyResolver.ResolveStrategy(sourceType);

            int xpAmount = await strategy.CalculateXpAsync(request.Dto.ReferenceId, cancellationToken);

            if (user.Data.IsPremium)
            {
                xpAmount = (int)(xpAmount * 1.5);
            }

            var transaction = new XpTransaction
            {
                ExplorerProfileId = request.Dto.ExplorerId,
                Amount = xpAmount,
                Source = sourceType,
                ReferenceId = request.Dto.ReferenceId
            };

            _repository.Add(transaction);
            await _repository.SaveChangesAsync(cancellationToken);

            // Update cache
            var userStats = await _mediator.Send(new GetUserStatsByExplorerIdQuery(request.Dto.ExplorerId), cancellationToken);
            await _cacheService.SortedSetAddAsync("leaderboard:xp", request.Dto.ExplorerId.ToString(), userStats.Data.CumulativeXp + xpAmount);

            _logger.LogInformation("XP transaction {TransactionId} created with {Amount} XP for explorer {ExplorerId}",
                transaction.Id, xpAmount, request.Dto.ExplorerId);

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
