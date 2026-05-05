using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.DTOs.Challenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Challenges.Queries
{
    public class GetChallengesByPlaceIdQueryHandler : IRequestHandler<GetChallengesByPlaceIdQuery, IEnumerable<GetChallengeDto>>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<GetChallengesByPlaceIdQueryHandler> _logger;

        public GetChallengesByPlaceIdQueryHandler(
            IGenericRepository<Challenge> repository,
            ILogger<GetChallengesByPlaceIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<GetChallengeDto>> Handle(GetChallengesByPlaceIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching challenges for place {PlaceId}", request.PlaceId);

            var challenges = await _repository.GetTable()
                .Where(c => c.PlaceId == request.PlaceId)
                .ToListAsync(cancellationToken);

            var dtos = ChallengeMapper.ToGetDtos(challenges);

            _logger.LogInformation("Retrieved {Count} challenges for place {PlaceId}", challenges.Count(), request.PlaceId);

            return dtos;
        }
    }
}
