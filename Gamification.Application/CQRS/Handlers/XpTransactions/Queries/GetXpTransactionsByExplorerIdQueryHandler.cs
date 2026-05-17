using Gamification.Application.CQRS.Queries.XpTransactions;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.XpTransactions.Queries
{
    public class GetXpTransactionsByExplorerIdQueryHandler : IRequestHandler<GetXpTransactionsByExplorerIdQuery, ApiResponse<PagedResult<GetXpTransactionDto>>>
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

        public async Task<ApiResponse<PagedResult<GetXpTransactionDto>>> Handle(GetXpTransactionsByExplorerIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching XP transactions for explorer {ExplorerId} - page {Page}, pageSize {PageSize}", request.ExplorerId, request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Where(t => t.ExplorerProfileId == request.ExplorerId)
                .Select(t => XpTransactionMapper.ToGetDto(t))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} transactions for explorer {ExplorerId} out of {TotalCount}", result.Items.Count(), request.ExplorerId, result.TotalCount);

            return ApiResponse<PagedResult<GetXpTransactionDto>>.Success(result);
        }
    }
}
