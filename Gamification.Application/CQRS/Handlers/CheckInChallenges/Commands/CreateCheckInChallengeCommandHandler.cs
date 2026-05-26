using Gamification.Application.CQRS.Commands.CheckInChallenges;
using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.CQRS.Queries.CheckInChallenge;
using Gamification.Application.DTOs.CheckInChallenge;
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
    public class CreateCheckInChallengeCommandHandler : IRequestHandler<CreateCheckInChallengeCommand, ApiResponse<GetCheckInChallengeDto>>
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

        public async Task<ApiResponse<GetCheckInChallengeDto>> Handle(CreateCheckInChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating check-in challenge for challenge {ChallengeId}", request.Dto.ChallengeId);

            var challenge = await _mediator.Send(new GetChallengeByIdQuery(request.Dto.ChallengeId), cancellationToken);
            if (!challenge.IsSuccess)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Dto.ChallengeId);
                return ApiResponse<GetCheckInChallengeDto>.Failure(ErrorCode.NotFound);
            }

            var CheckInChallengeExists = await _repository.GetTable().Where(c => c.ChallengeId == request.Dto.ChallengeId && c.CheckInId == request.Dto.CheckInId).AnyAsync(cancellationToken);
            if (CheckInChallengeExists)
            {
                _logger.LogWarning("CheckInChallenge ChallengeId: {ChallengeId}, CheckInId: {CheckInId} already exists", request.Dto.ChallengeId, request.Dto.CheckInId);
                return ApiResponse<GetCheckInChallengeDto>.Failure(ErrorCode.Conflict);
            }

            var checkInChallenge = CheckInChallengeMapper.ToEntity(request.Dto);
            checkInChallenge.ExplorerId = request.ExplorerId;
            _repository.Add(checkInChallenge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in challenge {CheckInChallengeId} created successfully", checkInChallenge.Id);

            var dto = CheckInChallengeMapper.ToGetDto(checkInChallenge);
            return ApiResponse<GetCheckInChallengeDto>.Success(dto);
        }
    }
}
