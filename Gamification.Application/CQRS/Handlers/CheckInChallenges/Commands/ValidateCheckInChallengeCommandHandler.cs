using Gamification.Application.CQRS.Commands.CheckInChallenge;
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
using System.Text;

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Commands
{
    public class ValidateCheckInChallengeCommandHandler : IRequestHandler<ValidateCheckInChallengeCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<CheckInChallenge> _repository;
        private readonly ILogger<ValidateCheckInChallengeCommandHandler> _logger;

        public ValidateCheckInChallengeCommandHandler(IGenericRepository<CheckInChallenge> repository, ILogger<ValidateCheckInChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(ValidateCheckInChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validating check-in challenge {CheckInChallengeId}", request.Id);

            var checkInChallengeExists = await _repository.GetTable().Where(c => c.Id == request.Id).AnyAsync(cancellationToken);
            if (!checkInChallengeExists)
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} not found", request.Id);
                return ApiResponse<bool>.Failure(ErrorCode.NotFound);
            }


            CheckInChallenge checkinChllange = new CheckInChallenge()
            {
                Id = request.Id
            };
            //TODO: Add your validation logic here
            bool isValid = true; // Replace with actual validation result

            if (isValid)
                checkinChllange.ValidationStatus = ChallengeValidationStatus.Approved;
            else
                checkinChllange.ValidationStatus = ChallengeValidationStatus.Rejected;

            _repository.SaveInclude(checkinChllange, nameof(CheckInChallenge.ValidationStatus));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in challenge {CheckInChallengeId} validation result: {IsValid}", request.Id, isValid);

            return ApiResponse<bool>.Success(isValid);
        }
    }
}
