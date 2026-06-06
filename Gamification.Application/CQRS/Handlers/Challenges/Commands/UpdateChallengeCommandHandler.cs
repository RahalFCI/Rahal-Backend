using Gamification.Application.CQRS.Commands.Challenge;
using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
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
    public class UpdateChallengeCommandHandler : IRequestHandler<UpdateChallengeCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<Challenge> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateChallengeCommandHandler> _logger;

        public UpdateChallengeCommandHandler(
            IGamificationRepository<Challenge> repository,
            IMediator mediator,
            ILogger<UpdateChallengeCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating challenge {ChallengeId}", request.Id);

            var existingChallenge = await _mediator.Send(new GetChallengeByNameQuery(request.Dto.Name), cancellationToken);
            if (existingChallenge.IsSuccess)
            {
                _logger.LogWarning("Challenge {ChallengeName} already exists", request.Dto.Name);
                return ApiResponse<string>.Failure(ErrorCode.Conflict);
            }

            var challenge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }

            ChallengeMapper.UpdateEntity(challenge, request.Dto);
            _repository.Update(challenge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Challenge {ChallengeId} updated successfully", request.Id);

            return ApiResponse<string>.Success("Challenge updated successfully");
        }
    }
}
