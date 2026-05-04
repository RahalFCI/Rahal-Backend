using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.DTOs.CheckInChallenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamification.Application.CQRS.Queries.CheckInChallenges
{
    public class GetCheckInChallengeByIdQueryHandler : IRequestHandler<GetCheckInChallengeByIdQuery, GetCheckInChallengeDto?>
    {
        private readonly IGenericRepository<CheckInChallenge> _repository;
        private readonly ILogger<GetCheckInChallengeByIdQueryHandler> _logger;

        public GetCheckInChallengeByIdQueryHandler(
            IGenericRepository<CheckInChallenge> repository,
            ILogger<GetCheckInChallengeByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetCheckInChallengeDto?> Handle(GetCheckInChallengeByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching check-in challenge {CheckInChallengeId}", request.Id);

            var checkInChallenge = await _repository.GetTable()
                .Where(c => c.Id == request.Id)
                .Include(c => c.Challenge)
                .FirstOrDefaultAsync(cancellationToken);

            if (checkInChallenge is null)
            {
                _logger.LogWarning("Check-in challenge {CheckInChallengeId} not found", request.Id);
                return null;
            }

            return CheckInChallengeMapper.ToGetDto(checkInChallenge);
        }
    }

    public class GetCheckInChallengesByCheckInIdQueryHandler : IRequestHandler<GetCheckInChallengesByCheckInIdQuery, IEnumerable<GetCheckInChallengeDto>>
    {
        private readonly IGenericRepository<CheckInChallenge> _repository;
        private readonly ILogger<GetCheckInChallengesByCheckInIdQueryHandler> _logger;

        public GetCheckInChallengesByCheckInIdQueryHandler(
            IGenericRepository<CheckInChallenge> repository,
            ILogger<GetCheckInChallengesByCheckInIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<GetCheckInChallengeDto>> Handle(GetCheckInChallengesByCheckInIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching check-in challenges for check-in {CheckInId}", request.CheckInId);

            var checkInChallenges = await _repository.GetTable()
                .Where(c => c.CheckInId == request.CheckInId)
                .Include(c => c.Challenge)
                .ToListAsync(cancellationToken);

            var dtos = CheckInChallengeMapper.ToGetDtos(checkInChallenges);

            _logger.LogInformation("Retrieved {Count} check-in challenges for check-in {CheckInId}", 
                checkInChallenges.Count(), request.CheckInId);

            return dtos;
        }
    }

    public class GetCheckInChallengesByChallengeIdQueryHandler : IRequestHandler<GetCheckInChallengesByChallengeIdQuery, IEnumerable<GetCheckInChallengeDto>>
    {
        private readonly IGenericRepository<Domain.Entities.CheckInChallenge> _repository;
        private readonly ILogger<GetCheckInChallengesByChallengeIdQueryHandler> _logger;

        public GetCheckInChallengesByChallengeIdQueryHandler(
            IGenericRepository<Domain.Entities.CheckInChallenge> repository,
            ILogger<GetCheckInChallengesByChallengeIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<GetCheckInChallengeDto>> Handle(GetCheckInChallengesByChallengeIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching check-in challenges for challenge {ChallengeId}", request.ChallengeId);

            var checkInChallenges = await _repository.GetTable()
                .Where(c => c.ChallengeId == request.ChallengeId)
                .Include(c => c.Challenge)
                .ToListAsync(cancellationToken);

            var dtos = CheckInChallengeMapper.ToGetDtos(checkInChallenges);

            _logger.LogInformation("Retrieved {Count} check-in challenges for challenge {ChallengeId}", 
                checkInChallenges.Count(), request.ChallengeId);

            return dtos;
        }
    }
}
