using Gamification.Application.CQRS.Commands.CheckInChallenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Commands
{
    public class CreateCheckInChallengeCommandHandler : IRequestHandler<CreateCheckInChallengeCommand, string>
    {
        private readonly IGenericRepository<Domain.Entities.CheckInChallenge> _repository;
        private readonly IGenericRepository<Domain.Entities.Challenge> _challengeRepository;
        private readonly ILogger<CreateCheckInChallengeCommandHandler> _logger;

        public CreateCheckInChallengeCommandHandler(
            IGenericRepository<CheckInChallenge> repository,
            IGenericRepository<Challenge> challengeRepository,
            ILogger<CreateCheckInChallengeCommandHandler> logger)
        {
            _repository = repository;
            _challengeRepository = challengeRepository;
            _logger = logger;
        }

        public async Task<string> Handle(CreateCheckInChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating check-in challenge for challenge {ChallengeId}", request.Dto.ChallengeId);

            var challenge = await _challengeRepository.GetByIdAsync(request.Dto.ChallengeId, cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Dto.ChallengeId);
                return "Challenge not found";
            }

            var checkInChallenge = CheckInChallengeMapper.ToEntity(request.Dto);
            _repository.Add(checkInChallenge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in challenge {CheckInChallengeId} created successfully", checkInChallenge.Id);

            return $"Check-in challenge created successfully. ID: {checkInChallenge.Id}";
        }
    }
}
