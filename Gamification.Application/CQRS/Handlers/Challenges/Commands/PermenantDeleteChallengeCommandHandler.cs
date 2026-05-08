using Gamification.Application.CQRS.Commands.Badges;
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

namespace Gamification.Application.CQRS.Handlers.Challenges.Commands
{
    public class PermenantDeleteChallengeCommandHandler : IRequestHandler<PermenantDeleteChallengeCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<PermenantDeleteChallengeCommandHandler> _logger;

        public PermenantDeleteChallengeCommandHandler(
            IGenericRepository<Challenge> repository,
            ILogger<PermenantDeleteChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(PermenantDeleteChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting challenge {ChallengeId}", request.Id);

            var challenge = await _repository.GetTable().Where(c => c.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }

            _repository.Delete(challenge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Challenge {ChallengeId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Challenge deleted successfully");
        }
    }
}
