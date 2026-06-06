using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.CQRS.Orchestrators.CheckInChallenge;
using Gamification.Application.CQRS.Orchestrators.UserStat;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Interfaces;
using Gamification.Application.Strategies;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Orchestrators
{
    public class ValidateCheckInChallengeOrchestratorHandler : IRequestHandler<ValidateCheckInChallengeOrchestrator, ApiResponse<bool>>
    {
        private readonly IGamificationRepository<CheckInChallenge> _repository;
        private readonly IGamificationUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly XpCalculationStrategyResolver _strategy;
        private readonly ILogger<ValidateCheckInChallengeOrchestratorHandler> _logger;

        public ValidateCheckInChallengeOrchestratorHandler(IGamificationRepository<CheckInChallenge> repository, IGamificationUnitOfWork unitOfWork, IMediator mediator, XpCalculationStrategyResolver strategy, ILogger<ValidateCheckInChallengeOrchestratorHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _strategy = strategy;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(ValidateCheckInChallengeOrchestrator request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validating check-in challenge {CheckInChallengeId}", request.Id);
            
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            //Get Checkin Challenge
            var checkInChallengeExists = await _repository.GetTable().Where(c => c.Id == request.Id).AnyAsync(cancellationToken);
            if (!checkInChallengeExists)
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} not found", request.Id);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<bool>.Failure(ErrorCode.NotFound);
            }

            //Validate using AI API
            //TODO: Add your validation logic here

            //Update checkin challenge status
            CheckInChallenge checkinChllange = new CheckInChallenge()
            {
                Id = request.Id
            };
            bool isValid = true; // Replace with actual validation result

            if (isValid)
                checkinChllange.ValidationStatus = ChallengeValidationStatus.Approved;
            else
                checkinChllange.ValidationStatus = ChallengeValidationStatus.Rejected;



            //Create Xp Transaction
            var xpTransactionResult = await _mediator.Send(new CreateXpTransactionCommand(new CreateXpTransactionDto()
            {
                ExplorerId = request.Id,
                ReferenceId = checkinChllange.Id,
                SourceType = XpSourceType.Challenge.ToString()

            }));
            if (!xpTransactionResult.IsSuccess)
            {
                _logger.LogError("Failed to create XP transaction for explorer {ExplorerId} after validating check-in challenge {CheckInChallengeId}", request.Id, request.Id);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<bool>.Failure(xpTransactionResult.errorCode);
            }

            //Update user stats
            var userStatsResult = await _mediator.Send(new UpdateChallengeStatsOrchestrator(request.Id, xpTransactionResult.Data.Amount), cancellationToken);
            if (!userStatsResult.IsSuccess)
            {
                _logger.LogError("Failed to update user stats for explorer {ExplorerId} after validating check-in challenge {CheckInChallengeId}", request.Id, request.Id);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<bool>.Failure(userStatsResult.errorCode);
            }

            _repository.SaveInclude(checkinChllange, nameof(CheckInChallenge.ValidationStatus));
            await _repository.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogInformation("Check-in challenge {CheckInChallengeId} validation result: {IsValid}", request.Id, isValid);

            return ApiResponse<bool>.Success(isValid);
        }
    }
}
