using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.DTOs.Challenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Challenges.Queries
{
    public class GetAllChallengesQueryHandler : IRequestHandler<GetAllChallengesQuery, ApiResponse<IEnumerable<GetChallengeDto>>>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<GetAllChallengesQueryHandler> _logger;

        public GetAllChallengesQueryHandler(
            IGenericRepository<Challenge> repository,
            ILogger<GetAllChallengesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<GetChallengeDto>>> Handle(GetAllChallengesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all challenges");

            var challenges = await _repository.GetAllAsync(cancellationToken: cancellationToken);
            var dtos = ChallengeMapper.ToGetDtos(challenges);

            _logger.LogInformation("Retrieved {Count} challenges", challenges.Count());

            return ApiResponse<IEnumerable<GetChallengeDto>>.Success(dtos);
        }
    }
}
