using Gamification.Application.CQRS.Queries.CheckInChallenge;
using Gamification.Application.DTOs.CheckInChallenge;
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

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Queries
{
    public class GetCheckInChallengesByCheckInIdQueryHandler : IRequestHandler<GetCheckInChallengesByCheckInIdQuery, ApiResponse<IEnumerable<GetCheckInChallengeDto>>>
    {
        private readonly IGenericRepository<CheckInChallenge> _repository;
        private readonly ILogger<GetCheckInChallengesByCheckInIdQueryHandler> _logger;

        public GetCheckInChallengesByCheckInIdQueryHandler(
            IGenericRepository<CheckInChallenge> repository,
            ILogger<GetCheckInChallengesByCheckInIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<GetCheckInChallengeDto>>> Handle(GetCheckInChallengesByCheckInIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching check-in challenges for check-in {CheckInId}", request.CheckInId);

            var checkInChallenges = await _repository.GetTable()
                .Where(c => c.CheckInId == request.CheckInId)
                .Include(c => c.Challenge)
                .ToListAsync(cancellationToken);

            var dtos = CheckInChallengeMapper.ToGetDtos(checkInChallenges);

            _logger.LogInformation("Retrieved {Count} check-in challenges for check-in {CheckInId}",
                checkInChallenges.Count(), request.CheckInId);

            return ApiResponse<IEnumerable<GetCheckInChallengeDto>>.Success(dtos);
        }
    }
}
