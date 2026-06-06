using Gamification.Application.CQRS.Commands.CheckInChallenges;
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
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Commands
{
    public class DeleteCheckInChallengeCommandHandler : IRequestHandler<DeleteCheckInChallengeCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<CheckInChallenge> _repository;
        private readonly ILogger<DeleteCheckInChallengeCommandHandler> _logger;

        public DeleteCheckInChallengeCommandHandler(
            IGamificationRepository<CheckInChallenge> repository,
            ILogger<DeleteCheckInChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteCheckInChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting check-in challenge {CheckInChallengeId}", request.Id);

            var checkInChallengeExists = await _repository.GetTable().Where(c => c.Id == request.Id).AnyAsync(cancellationToken);
            if (!checkInChallengeExists)
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            CheckInChallenge checkInChallenge = new CheckInChallenge
            {
                Id = request.Id,
                DeletedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            _repository.SaveInclude(checkInChallenge, nameof(CheckInChallenge.IsDeleted), nameof(CheckInChallenge.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in challenge {CheckInChallengeId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Check-in challenge deleted successfully");
        }
    }
}
