using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.DTOs.Challenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Challenges.Queries
{
    public class GetChallengeByIdQueryHandler : IRequestHandler<GetChallengeByIdQuery, GetChallengeDto?>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<GetChallengeByIdQueryHandler> _logger;

        public GetChallengeByIdQueryHandler(
            IGenericRepository<Challenge> repository,
            ILogger<GetChallengeByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetChallengeDto?> Handle(GetChallengeByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching challenge {ChallengeId}", request.Id);

            var challenge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Id);
                return null;
            }

            return ChallengeMapper.ToGetDto(challenge);
        }
    }
}
