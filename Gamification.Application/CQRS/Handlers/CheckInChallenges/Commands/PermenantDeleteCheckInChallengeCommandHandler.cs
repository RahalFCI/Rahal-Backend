using Gamification.Application.CQRS.Commands.CheckInChallenges;
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

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Commands
{
    public class PermenantDeleteCheckInChallengeCommandHandler : IRequestHandler<PermenantDeleteCheckInChallengeCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<CheckInChallenge> _repository;
        private readonly ILogger<PermenantDeleteCheckInChallengeCommandHandler> _logger;

        public PermenantDeleteCheckInChallengeCommandHandler(
            IGamificationRepository<CheckInChallenge> repository,
            ILogger<PermenantDeleteCheckInChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(PermenantDeleteCheckInChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting check-in challenge {CheckInChallengeId}", request.Id);

            var checkInChallenge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (checkInChallenge is null)
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            _repository.Delete(checkInChallenge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in challenge {CheckInChallengeId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Check-in challenge deleted successfully");
        }
    }
}
