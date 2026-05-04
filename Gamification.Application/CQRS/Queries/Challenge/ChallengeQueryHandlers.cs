using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.DTOs.Challenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamification.Application.CQRS.Queries.Challenges
{
    public class GetChallengeByIdQueryHandler : IRequestHandler<GetChallengeByIdQuery, GetChallengeDto?>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<GetChallengeByIdQueryHandler> _logger;

        public GetChallengeByIdQueryHandler(
            IGenericRepository<Challenge> repository,
            ILogger<GetChallengeByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetChallengeDto?> Handle(GetChallengeByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching challenge {ChallengeId}", request.Id);

            var challenge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", request.Id);
                return null;
            }

            return ChallengeMapper.ToGetDto(challenge);
        }
    }

    public class GetAllChallengesQueryHandler : IRequestHandler<GetAllChallengesQuery, IEnumerable<GetChallengeDto>>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<GetAllChallengesQueryHandler> _logger;

        public GetAllChallengesQueryHandler(
            IGenericRepository<Challenge> repository,
            ILogger<GetAllChallengesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<GetChallengeDto>> Handle(GetAllChallengesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all challenges");

            var challenges = await _repository.GetAllAsync(cancellationToken: cancellationToken);
            var dtos = ChallengeMapper.ToGetDtos(challenges);

            _logger.LogInformation("Retrieved {Count} challenges", challenges.Count());

            return dtos;
        }
    }

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
