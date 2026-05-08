using Gamification.Application.CQRS.Commands.CheckInChallenge;
using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.CQRS.Queries.CheckInChallenge;
using Gamification.Application.Mappers;
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

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Commands
{
    public class CreateCheckInChallengeCommandHandler : IRequestHandler<CreateCheckInChallengeCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.CheckInChallenge> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateCheckInChallengeCommandHandler> _logger;

        public CreateCheckInChallengeCommandHandler(
            IGenericRepository<CheckInChallenge> repository,
            IMediator mediator,
            ILogger<CreateCheckInChallengeCommandHandler> logger)
        {
            _repository = repository; 
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(CreateCheckInChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating check-in challenge for challenge {ChallengeId}", request.Dto.ChallengeId);

            var challenge = await _mediator.Send(new GetChallengeByIdQuery(request.Dto.ChallengeId), cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Dto.ChallengeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var CheckInChallengeExists = await _repository.GetTable().Where(c => c.ChallengeId == request.Dto.ChallengeId && c.CheckInId == request.Dto.CheckInId).AnyAsync(cancellationToken);
            if (CheckInChallengeExists)
            {
                _logger.LogWarning("CheckInChallenge ChallengeId: {ChallengeId}, CheckInId: {CheckInId} already exists", request.Dto.ChallengeId, request.Dto.CheckInId);
                return ApiResponse<string>.Failure(ErrorCode.Conflict);
            }

            var checkInChallenge = CheckInChallengeMapper.ToEntity(request.Dto);
            _repository.Add(checkInChallenge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in challenge {CheckInChallengeId} created successfully", checkInChallenge.Id);

            return ApiResponse<string>.Success($"Check-in challenge created successfully. ID: {checkInChallenge.Id}");
        }
    }
}
