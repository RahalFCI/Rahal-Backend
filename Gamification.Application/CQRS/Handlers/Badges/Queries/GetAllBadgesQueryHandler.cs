using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.DTOs.Badge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.Badges.Queries
{
    public class GetAllBadgesQueryHandler : IRequestHandler<GetAllBadgesQuery, ApiResponse<PagedResult<GetBadgeDto>>>
    {
        private readonly IGamificationRepository<Badge> _repository;
        private readonly ILogger<GetAllBadgesQueryHandler> _logger;

        public GetAllBadgesQueryHandler(
            IGamificationRepository<Badge> repository,
            ILogger<GetAllBadgesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetBadgeDto>>> Handle(GetAllBadgesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all badges - page {Page}, pageSize {PageSize}", request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Select(b => BadgeMapper.ToGetDto(b))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} badges out of {TotalCount}", result.Items.Count(), result.TotalCount);

            return ApiResponse<PagedResult<GetBadgeDto>>.Success(result);
        }
    }
}
