using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamification.Application.CQRS.Queries.XpTransactions
{
    public class GetXpTransactionsByExplorerIdQueryHandler : IRequestHandler<GetXpTransactionsByExplorerIdQuery, IEnumerable<GetXpTransactionDto>>
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

        public async Task<IEnumerable<GetXpTransactionDto>> Handle(GetXpTransactionsByExplorerIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching XP transactions for explorer {ExplorerId}", request.ExplorerId);

            var transactions = await _repository.GetTable()
                .Where(t => t.ExplorerProfileId == request.ExplorerId)
                .ToListAsync(cancellationToken);

            var dtos = XpTransactionMapper.ToGetDtos(transactions);

            _logger.LogInformation("Retrieved {Count} transactions for explorer {ExplorerId}", transactions.Count(), request.ExplorerId);

            return dtos;
        }
    }
}
