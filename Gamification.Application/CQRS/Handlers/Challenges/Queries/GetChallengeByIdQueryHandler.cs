using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.DTOs.Challenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.Challenges.Queries
{
    public class GetChallengeByIdQueryHandler : IRequestHandler<GetChallengeByIdQuery, ApiResponse<GetChallengeDto>>
    {
        private readonly IGamificationRepository<Challenge> _repository;
        private readonly ILogger<GetChallengeByIdQueryHandler> _logger;

        public GetChallengeByIdQueryHandler(
            IGamificationRepository<Challenge> repository,
            ILogger<GetChallengeByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetChallengeDto>> Handle(GetChallengeByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching challenge {ChallengeId}", request.Id);

            var challenge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Id);
                return ApiResponse<GetChallengeDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<GetChallengeDto>.Success(ChallengeMapper.ToGetDto(challenge));
        }
    }
}
