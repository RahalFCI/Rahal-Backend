using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.DTOs.Challenge;
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

namespace Gamification.Application.CQRS.Handlers.Challenges.Queries
{
    public class GetChallengeByNameQueryHandler : IRequestHandler<GetChallengeByNameQuery, ApiResponse<GetChallengeDto>>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<GetChallengeByNameQueryHandler> _logger;

        public GetChallengeByNameQueryHandler(
            IGenericRepository<Challenge> repository,
            ILogger<GetChallengeByNameQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetChallengeDto>> Handle(GetChallengeByNameQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching challenge {ChallengeName}", request.Name);

            var challenge = await _repository.GetTable().Where(c => c.Name == request.Name).FirstOrDefaultAsync(cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeName} not found", request.Name);
                return ApiResponse<GetChallengeDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<GetChallengeDto>.Success(ChallengeMapper.ToGetDto(challenge));
        }
    }
}
