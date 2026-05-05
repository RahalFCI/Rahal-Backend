using Gamification.Application.CQRS.Commands.CheckInChallenge;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Commands
{
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
