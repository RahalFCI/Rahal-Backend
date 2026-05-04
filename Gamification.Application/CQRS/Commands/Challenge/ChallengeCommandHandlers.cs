using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.DTOs.Challenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;

namespace Gamification.Application.CQRS.Commands.Challenges
{
    public class CreateChallengeCommandHandler : IRequestHandler<CreateChallengeCommand, string>
    {
        private readonly IGenericRepository<Domain.Entities.Challenge> _repository;
        private readonly ILogger<CreateChallengeCommandHandler> _logger;

        public CreateChallengeCommandHandler(
            IGenericRepository<Domain.Entities.Challenge> repository,
            ILogger<CreateChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(CreateChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating challenge {ChallengeName} for place {PlaceId}", 
                request.Dto.Name, request.Dto.PlaceId);

            var challenge = ChallengeMapper.ToEntity(request.Dto);
            _repository.Add(challenge);
            await _repository.SaveChangesAsync(cancellationToken);
            

            _logger.LogInformation("Challenge {ChallengeId} created successfully", challenge.Id);

            return $"Challenge created successfully. ID: {challenge.Id}";
        }
    }

    public class UpdateChallengeCommandHandler : IRequestHandler<UpdateChallengeCommand, string>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<UpdateChallengeCommandHandler> _logger;

        public UpdateChallengeCommandHandler(
            IGenericRepository<Challenge> repository,
            ILogger<UpdateChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(UpdateChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating challenge {ChallengeId}", request.Id);

            var challenge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Id);
                return "Challenge not found";
            }

            ChallengeMapper.UpdateEntity(challenge, request.Dto);
            _repository.Update(challenge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Challenge {ChallengeId} updated successfully", request.Id);

            return "Challenge updated successfully";
        }
    }

    public class DeleteChallengeCommandHandler : IRequestHandler<DeleteChallengeCommand, string>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<DeleteChallengeCommandHandler> _logger;

        public DeleteChallengeCommandHandler(
            IGenericRepository<Challenge> repository,
            ILogger<DeleteChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(DeleteChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting challenge {ChallengeId}", request.Id);

            var challenge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Id);
                return "Challenge not found";
            }

            _repository.Delete(challenge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Challenge {ChallengeId} deleted successfully", request.Id);

            return "Challenge deleted successfully";
        }
    }
}
