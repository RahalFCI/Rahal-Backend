using Gamification.Application.CQRS.Queries.CheckInChallenge;
using Gamification.Application.DTOs.CheckInChallenge;
using Gamification.Application.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Queries
{
    public class GetCheckInChallengesByChallengeIdQueryHandler : IRequestHandler<GetCheckInChallengesByChallengeIdQuery, ApiResponse<IEnumerable<GetCheckInChallengeDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.CheckInChallenge> _repository;
        private readonly ILogger<GetCheckInChallengesByChallengeIdQueryHandler> _logger;

        public GetCheckInChallengesByChallengeIdQueryHandler(
            IGenericRepository<Domain.Entities.CheckInChallenge> repository,
            ILogger<GetCheckInChallengesByChallengeIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<GetCheckInChallengeDto>>> Handle(GetCheckInChallengesByChallengeIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching check-in challenges for challenge {ChallengeId}", request.ChallengeId);

            var checkInChallenges = await _repository.GetTable()
                .Where(c => c.ChallengeId == request.ChallengeId)
                .Include(c => c.Challenge)
                .ToListAsync(cancellationToken);

            var dtos = CheckInChallengeMapper.ToGetDtos(checkInChallenges);

            _logger.LogInformation("Retrieved {Count} check-in challenges for challenge {ChallengeId}",
                checkInChallenges.Count(), request.ChallengeId);

            return ApiResponse<IEnumerable<GetCheckInChallengeDto>>.Success(dtos);
        }
    }
}
