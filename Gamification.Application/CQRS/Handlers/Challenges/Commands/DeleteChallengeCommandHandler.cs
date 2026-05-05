using Gamification.Application.CQRS.Commands.Challenge;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Challenges.Commands
{
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
