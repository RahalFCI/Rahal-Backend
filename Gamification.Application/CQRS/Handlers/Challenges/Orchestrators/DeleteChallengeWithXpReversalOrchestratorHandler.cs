using Gamification.Application.CQRS.Commands.Challenge;
using Gamification.Application.CQRS.Orchestrators.Challenges;
using Gamification.Application.Jobs;
using Gamification.Domain.Entities;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.Challenges.Orchestrators
{
    public class DeleteChallengeWithXpReversalOrchestratorHandler : IRequestHandler<DeleteChallengeWithXpReversalOrchestrator, ApiResponse<string>>
    {
        private readonly IGenericRepository<Challenge> _challengeRepository;
        private readonly IMediator _mediator;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<DeleteChallengeWithXpReversalOrchestratorHandler> _logger;

        public DeleteChallengeWithXpReversalOrchestratorHandler(
            IGenericRepository<Challenge> challengeRepository,
            IMediator mediator,
            IBackgroundJobClient backgroundJobClient,
            ILogger<DeleteChallengeWithXpReversalOrchestratorHandler> logger)
        {
            _challengeRepository = challengeRepository;
            _mediator = mediator;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteChallengeWithXpReversalOrchestrator request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting challenge deletion with XP reversal for challenge {ChallengeId}", request.ChallengeId);

            var challenge = await _challengeRepository.GetTable()
                .Where(c => c.Id == request.ChallengeId)
                .FirstOrDefaultAsync(cancellationToken);

            if (challenge == null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.ChallengeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var xpReward = challenge.XpReward;

            var deleteResult = await _mediator.Send(new DeleteChallengeCommand(request.ChallengeId), cancellationToken);
            if (!deleteResult.IsSuccess)
            {
                _logger.LogError("Failed to delete challenge {ChallengeId}. Error: {ErrorCode}", request.ChallengeId, deleteResult.errorCode);
                return deleteResult;
            }

            _backgroundJobClient.Enqueue<ChallengeXpReversalJob>(j => j.ExecuteAsync(request.ChallengeId, xpReward, CancellationToken.None));

            _logger.LogInformation("Challenge {ChallengeId} deleted successfully. XP reversal job enqueued", request.ChallengeId);
            return ApiResponse<string>.Success("Challenge deleted successfully");
        }
    }
}
