using Gamification.Application.CQRS.Commands.CheckInChallenges;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Commands
{
    public class RestoreDeletedCheckInChallengeCommandHandler : IRequestHandler<RestoreDeletedCheckInChallengeCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<CheckInChallenge> _repository;
        private readonly ILogger<RestoreDeletedCheckInChallengeCommandHandler> _logger;

        public RestoreDeletedCheckInChallengeCommandHandler(
            IGenericRepository<CheckInChallenge> repository,
            ILogger<RestoreDeletedCheckInChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedCheckInChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring deleted check-in challenge {CheckInChallengeId}", request.Id);

            var checkInChallengeExists = await _repository.GetTable()
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == request.Id && c.IsDeleted, cancellationToken);

            if (!checkInChallengeExists)
            {
                _logger.LogWarning("Deleted check-in challenge {CheckInChallengeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            CheckInChallenge checkInChallenge = new CheckInChallenge()
            {
                Id = request.Id,
                IsDeleted = false,
                DeletedAt = null
            };

            _repository.SaveInclude(checkInChallenge, nameof(checkInChallenge.IsDeleted), nameof(checkInChallenge.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in challenge {CheckInChallengeId} restored successfully", request.Id);

            return ApiResponse<string>.Success("Check-in challenge restored successfully");
        }
    }
}
