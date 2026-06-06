using Gamification.Application.CQRS.Commands.Challenge;
using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.DTOs.Challenge;
using Gamification.Application.Mappers;
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
    public class CreateChallengeCommandHandler : IRequestHandler<CreateChallengeCommand, ApiResponse<GetChallengeDto>>
    {
        private readonly IGamificationRepository<Domain.Entities.Challenge> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateChallengeCommandHandler> _logger;

        public CreateChallengeCommandHandler(
            IGamificationRepository<Domain.Entities.Challenge> repository,
            IMediator mediator,
            ILogger<CreateChallengeCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<GetChallengeDto>> Handle(CreateChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating challenge {ChallengeName} for place {PlaceId}",
                request.Dto.Name, request.Dto.PlaceId);

            var existingChallenge = await _mediator.Send(new GetChallengeByNameQuery(request.Dto.Name), cancellationToken);
            if (existingChallenge.IsSuccess)
            {
                _logger.LogWarning("Challenge {ChallengeName} already exists", request.Dto.Name);
                return ApiResponse<GetChallengeDto>.Failure(ErrorCode.Conflict);
            }

            var challenge = ChallengeMapper.ToEntity(request.Dto);
            _repository.Add(challenge);
            await _repository.SaveChangesAsync(cancellationToken);


            _logger.LogInformation("Challenge {ChallengeId} created successfully", challenge.Id);

            var dto = ChallengeMapper.ToGetDto(challenge);
            return ApiResponse<GetChallengeDto>.Success(dto);
        }
    }
}
