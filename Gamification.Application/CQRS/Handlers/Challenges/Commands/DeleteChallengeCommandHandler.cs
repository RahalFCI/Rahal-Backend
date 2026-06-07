using Gamification.Application.CQRS.Commands.Challenge;
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

namespace Gamification.Application.CQRS.Handlers.Challenges.Commands
{
    public class DeleteChallengeCommandHandler : IRequestHandler<DeleteChallengeCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<Challenge> _repository;
        private readonly ILogger<DeleteChallengeCommandHandler> _logger;

        public DeleteChallengeCommandHandler(
            IGamificationRepository<Challenge> repository,
            ILogger<DeleteChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting challenge {ChallengeId}", request.Id);

            var challengeExists = await _repository.GetTable().Where(c => c.Id == request.Id).AnyAsync(cancellationToken);
            if (!challengeExists)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            Challenge challenge = new() {
                Id = request.Id,
                DeletedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            _repository.SaveInclude(challenge, nameof(challenge.IsDeleted), nameof(challenge.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Challenge {ChallengeId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Challenge deleted successfully");
        }
    }
}
