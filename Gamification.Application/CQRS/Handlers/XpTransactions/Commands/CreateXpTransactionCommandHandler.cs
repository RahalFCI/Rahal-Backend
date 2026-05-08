using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.Strategies;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
namespace Gamification.Application.CQRS.Handlers.XpTransactions.Commands
{
    public class CreateXpTransactionCommandHandler : IRequestHandler<CreateXpTransactionCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.XpTransaction> _repository;
        private readonly XpCalculationStrategyResolver _strategyResolver;
        private readonly ILogger<CreateXpTransactionCommandHandler> _logger;

        public CreateXpTransactionCommandHandler(
            IGenericRepository<XpTransaction> repository,
            XpCalculationStrategyResolver strategyResolver,
            ILogger<CreateXpTransactionCommandHandler> logger)
        {
            _repository = repository;
            _strategyResolver = strategyResolver;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(CreateXpTransactionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating XP transaction for explorer {ExplorerId}", request.Dto.ExplorerId);

            var sourceType = Enum.Parse<XpSourceType>(request.Dto.SourceType);
            var strategy = _strategyResolver.ResolveStrategy(sourceType);

            int xpAmount;
            if (request.Dto.ReferenceId != Guid.Empty)
            {
                xpAmount = await strategy.CalculateXpAsync(request.Dto.ReferenceId, cancellationToken);
            }
            else
            {
                xpAmount = request.Dto.Amount;
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

            _logger.LogInformation("XP transaction {TransactionId} created with {Amount} XP for explorer {ExplorerId}",
                transaction.Id, xpAmount, request.Dto.ExplorerId);

            return ApiResponse<string>.Success($"XP transaction created successfully. XP gained: {xpAmount}");
        }
    }
}
