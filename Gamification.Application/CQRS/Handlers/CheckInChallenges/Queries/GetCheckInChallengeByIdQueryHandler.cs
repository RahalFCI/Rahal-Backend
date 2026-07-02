using Gamification.Application.CQRS.Queries.CheckInChallenge;
using Gamification.Application.DTOs.CheckInChallenge;
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

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Queries
{
    public class GetCheckInChallengeByIdQueryHandler : IRequestHandler<GetCheckInChallengeByIdQuery, ApiResponse<GetCheckInChallengeDto>>
    {
        private readonly IGamificationRepository<CheckInChallenge> _repository;
        private readonly IGamificationRepository<ExplorerProfile> _explorerProfileRepository;
        private readonly ILogger<GetCheckInChallengeByIdQueryHandler> _logger;

        public GetCheckInChallengeByIdQueryHandler(
            IGamificationRepository<CheckInChallenge> repository,
            IGamificationRepository<ExplorerProfile> explorerProfileRepository,
            ILogger<GetCheckInChallengeByIdQueryHandler> logger)
        {
            _repository = repository;
            _explorerProfileRepository = explorerProfileRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetCheckInChallengeDto>> Handle(GetCheckInChallengeByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching check-in challenge {CheckInChallengeId}", request.Id);

            var checkInChallenge = await _repository.GetTable()
                .Where(c => c.Id == request.Id)
                .Include(c => c.Challenge)
                .FirstOrDefaultAsync(cancellationToken);

            if (checkInChallenge is null)
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} not found", request.Id);
                return ApiResponse<GetCheckInChallengeDto>.Failure(ErrorCode.NotFound);
            }

            // ExplorerProfile's key is UserId, not Id (see GetExplorerNamesByIdsQueryHandler).
            var explorerProfile = await _explorerProfileRepository.GetTable()
                .FirstOrDefaultAsync(ep => ep.UserId == checkInChallenge.ExplorerId, cancellationToken);

            return ApiResponse<GetCheckInChallengeDto>.Success(
                CheckInChallengeMapper.ToGetDto(checkInChallenge, explorerProfile?.DisplayName ?? string.Empty));
        }
    }
}
