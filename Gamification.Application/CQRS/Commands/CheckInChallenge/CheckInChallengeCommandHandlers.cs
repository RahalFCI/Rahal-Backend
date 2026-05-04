using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.DTOs.CheckInChallenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;

namespace Gamification.Application.CQRS.Commands.CheckInChallenges
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

    public class DeleteCheckInChallengeCommandHandler : IRequestHandler<DeleteCheckInChallengeCommand, string>
    {
        private readonly IGenericRepository<CheckInChallenge> _repository;
        private readonly ILogger<DeleteCheckInChallengeCommandHandler> _logger;

        public DeleteCheckInChallengeCommandHandler(
            IGenericRepository<CheckInChallenge> repository,
            ILogger<DeleteCheckInChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(DeleteCheckInChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting check-in challenge {CheckInChallengeId}", request.Id);

            var checkInChallenge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (checkInChallenge is null)
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} not found", request.Id);
                return "Check-in challenge not found";
            }

            _repository.Delete(checkInChallenge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in challenge {CheckInChallengeId} deleted successfully", request.Id);

            return "Check-in challenge deleted successfully";
        }
    }
}
