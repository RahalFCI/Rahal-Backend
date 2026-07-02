using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.CQRS.Orchestrators.CheckInChallenge;
using Gamification.Application.CQRS.Orchestrators.UserStat;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Interfaces;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Orchestrators
{
    public class ValidateCheckInChallengeOrchestratorHandler : IRequestHandler<ValidateCheckInChallengeOrchestrator, ApiResponse<bool>>
    {
        private readonly IGamificationRepository<CheckInChallenge> _repository;
        private readonly IGamificationUnitOfWork _unitOfWork;
        private readonly ICheckInChallengeAiValidationService _aiValidationService;
        private readonly IMediator _mediator;
        private readonly ILogger<ValidateCheckInChallengeOrchestratorHandler> _logger;

        public ValidateCheckInChallengeOrchestratorHandler(
            IGamificationRepository<CheckInChallenge> repository,
            IGamificationUnitOfWork unitOfWork,
            ICheckInChallengeAiValidationService aiValidationService,
            IMediator mediator,
            ILogger<ValidateCheckInChallengeOrchestratorHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _aiValidationService = aiValidationService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(ValidateCheckInChallengeOrchestrator request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validating check-in challenge {CheckInChallengeId}", request.Id);

            if (request.Image is null || request.Image.Length == 0)
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} validation image is missing", request.Id);
                return ApiResponse<bool>.Failure(ErrorCode.InvalidRequest);
            }

            var checkInChallenge = await _repository.GetTable()
                .Include(c => c.Challenge)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (checkInChallenge is null)
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} not found", request.Id);
                return ApiResponse<bool>.Failure(ErrorCode.NotFound);
            }

            var validationDescription = !string.IsNullOrWhiteSpace(checkInChallenge.Challenge?.ValidationPrompt)
                ? checkInChallenge.Challenge.ValidationPrompt
                : checkInChallenge.Challenge?.Description;

            if (string.IsNullOrWhiteSpace(validationDescription))
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} has no validation description", request.Id);
                return ApiResponse<bool>.Failure(ErrorCode.InvalidRequest);
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var validationResult = await _aiValidationService.ValidateCheckInChallengeAsync(
                request.Image,
                validationDescription,
                cancellationToken);

            if (!validationResult.IsSuccess)
            {
                _logger.LogWarning(
                    "AI validation failed for check-in challenge {CheckInChallengeId}. ErrorCode {ErrorCode}",
                    request.Id,
                    validationResult.errorCode);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<bool>.Failure(validationResult.errorCode);
            }

            var isValid = validationResult.Data;
            checkInChallenge.ValidationStatus = isValid
                ? ChallengeValidationStatus.Approved
                : ChallengeValidationStatus.Rejected;

            if (!isValid)
            {
                await _repository.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Check-in challenge {CheckInChallengeId} rejected by AI validation", request.Id);
                return ApiResponse<bool>.Success(false);
            }

            var xpTransactionResult = await _mediator.Send(new CreateXpTransactionCommand(new CreateXpTransactionDto
            {
                ExplorerId = checkInChallenge.ExplorerId,
                ReferenceId = checkInChallenge.ChallengeId,
                SourceType = XpSourceType.Challenge.ToString()
            }), cancellationToken);

            if (!xpTransactionResult.IsSuccess)
            {
                _logger.LogError(
                    "Failed to create XP transaction for explorer {ExplorerId} after validating check-in challenge {CheckInChallengeId}",
                    checkInChallenge.ExplorerId,
                    request.Id);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<bool>.Failure(xpTransactionResult.errorCode);
            }

            var userStatsResult = await _mediator.Send(
                new UpdateChallengeStatsOrchestrator(checkInChallenge.ExplorerId, xpTransactionResult.Data.Amount),
                cancellationToken);

            if (!userStatsResult.IsSuccess)
            {
                _logger.LogError(
                    "Failed to update user stats for explorer {ExplorerId} after validating check-in challenge {CheckInChallengeId}",
                    checkInChallenge.ExplorerId,
                    request.Id);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<bool>.Failure(userStatsResult.errorCode);
            }

            await _repository.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Check-in challenge {CheckInChallengeId} approved by AI validation", request.Id);
            return ApiResponse<bool>.Success(true);
        }
    }
}
