using Gamification.Application.CQRS.Queries.XpTransactions;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.XpTransactions.Queries
{
    public class GetXpTransactionsByExplorerIdQueryHandler : IRequestHandler<GetXpTransactionsByExplorerIdQuery, ApiResponse<IEnumerable<GetXpTransactionDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.XpTransaction> _repository;
        private readonly ILogger<GetXpTransactionsByExplorerIdQueryHandler> _logger;

        public GetXpTransactionsByExplorerIdQueryHandler(
            IGenericRepository<XpTransaction> repository,
            ILogger<GetXpTransactionsByExplorerIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<GetXpTransactionDto>>> Handle(GetXpTransactionsByExplorerIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching XP transactions for explorer {ExplorerId}", request.ExplorerId);

            var transactions = await _repository.GetTable()
                .Where(t => t.ExplorerProfileId == request.ExplorerId)
                .ToListAsync(cancellationToken);

            var dtos = XpTransactionMapper.ToGetDtos(transactions);

            _logger.LogInformation("Retrieved {Count} transactions for explorer {ExplorerId}", transactions.Count(), request.ExplorerId);

            return ApiResponse<IEnumerable<GetXpTransactionDto>>.Success(dtos);
        }
    }
}
