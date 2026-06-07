using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.DTOs.Badge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
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

namespace Gamification.Application.CQRS.Handlers.Badges.Queries
{
    public class GetBadgeByNameQueryHandler : IRequestHandler<GetBadgeByNameQuery, ApiResponse<GetBadgeDto>>
    {
        private readonly IGamificationRepository<Badge> _repository;
        private readonly ILogger<GetBadgeByNameQueryHandler> _logger;

        public GetBadgeByNameQueryHandler(
            IGamificationRepository<Badge> repository,
            ILogger<GetBadgeByNameQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetBadgeDto>> Handle(GetBadgeByNameQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching badge {BadgeName}", request.Name);

            var badge = await _repository.GetTable().Where(b => b.Name == request.Name).FirstOrDefaultAsync(cancellationToken);
            if (badge is null)
            {
                _logger.LogWarning("Badge {BadgeName} not found", request.Name);
                return ApiResponse<GetBadgeDto>.Failure(ErrorCode.NotFound);
            }

            var badgeDto = BadgeMapper.ToGetDto(badge);

            return ApiResponse<GetBadgeDto>.Success(badgeDto);
        }
    }
}
