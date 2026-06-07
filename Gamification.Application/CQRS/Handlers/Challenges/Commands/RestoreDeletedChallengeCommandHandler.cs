using Gamification.Application.CQRS.Commands.Challenge;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.Challenges.Commands
{
    public class RestoreDeletedChallengeCommandHandler : IRequestHandler<RestoreDeletedChallengeCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<Challenge> _repository;
        private readonly ILogger<RestoreDeletedChallengeCommandHandler> _logger;

        public RestoreDeletedChallengeCommandHandler(
            IGamificationRepository<Challenge> repository,
            ILogger<RestoreDeletedChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring deleted challenge {ChallengeId}", request.Id);

            var challengeExists = await _repository.GetTable()
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == request.Id && c.IsDeleted, cancellationToken);

            if (!challengeExists)
            {
                _logger.LogWarning("Deleted challenge {ChallengeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            Challenge challenge = new Challenge()
            {
                Id = request.Id,
                IsDeleted = false,
                DeletedAt = null
            };

            _repository.SaveInclude(challenge, nameof(challenge.IsDeleted), nameof(challenge.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Challenge {ChallengeId} restored successfully", request.Id);

            return ApiResponse<string>.Success("Challenge restored successfully");
        }
    }
}
