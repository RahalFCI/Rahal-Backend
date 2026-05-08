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

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Queries
{
    public class GetCheckInChallengeByIdQueryHandler : IRequestHandler<GetCheckInChallengeByIdQuery, ApiResponse<GetCheckInChallengeDto>>
    {
        private readonly IGenericRepository<CheckInChallenge> _repository;
        private readonly ILogger<GetCheckInChallengeByIdQueryHandler> _logger;

        public GetCheckInChallengeByIdQueryHandler(
            IGenericRepository<CheckInChallenge> repository,
            ILogger<GetCheckInChallengeByIdQueryHandler> logger)
        {
            _repository = repository;
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

            return ApiResponse<GetCheckInChallengeDto>.Success(CheckInChallengeMapper.ToGetDto(checkInChallenge));
        }
    }
}
