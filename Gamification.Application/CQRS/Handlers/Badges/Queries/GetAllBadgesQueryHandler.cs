using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.DTOs.Badge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Badges.Queries
{
    public class GetAllBadgesQueryHandler : IRequestHandler<GetAllBadgesQuery, ApiResponse<IEnumerable<GetBadgeDto>>>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly ILogger<GetAllBadgesQueryHandler> _logger;

        public GetAllBadgesQueryHandler(
            IGenericRepository<Badge> repository,
            ILogger<GetAllBadgesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<GetBadgeDto>>> Handle(GetAllBadgesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all badges");

            var badges = await _repository.GetAllAsync(cancellationToken: cancellationToken);
            var dtos = BadgeMapper.ToGetDtos(badges);

            _logger.LogInformation("Retrieved {Count} badges", badges.Count());

            return ApiResponse<IEnumerable<GetBadgeDto>>.Success(dtos);
        }
    }
}
