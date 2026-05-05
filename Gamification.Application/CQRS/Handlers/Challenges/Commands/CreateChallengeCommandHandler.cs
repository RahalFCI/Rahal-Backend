using Gamification.Application.CQRS.Commands.Challenge;
using Gamification.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Challenges.Commands
{
    public class CreateChallengeCommandHandler : IRequestHandler<CreateChallengeCommand, string>
    {
        private readonly IGenericRepository<Domain.Entities.Challenge> _repository;
        private readonly ILogger<CreateChallengeCommandHandler> _logger;

        public CreateChallengeCommandHandler(
            IGenericRepository<Domain.Entities.Challenge> repository,
            ILogger<CreateChallengeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(CreateChallengeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating challenge {ChallengeName} for place {PlaceId}",
                request.Dto.Name, request.Dto.PlaceId);

            var challenge = ChallengeMapper.ToEntity(request.Dto);
            _repository.Add(challenge);
            await _repository.SaveChangesAsync(cancellationToken);


            _logger.LogInformation("Challenge {ChallengeId} created successfully", challenge.Id);

            return $"Challenge created successfully. ID: {challenge.Id}";
        }
    }
}
