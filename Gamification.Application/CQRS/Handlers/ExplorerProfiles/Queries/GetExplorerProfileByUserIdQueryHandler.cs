using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.DTOs.Explorer;
using Gamification.Application.DTOs.Vendor;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries
{
    public class GetExplorerProfileByUserIdQueryHandler : IRequestHandler<GetExplorerProfileByUserIdQuery, ApiResponse<GetExplorerDto>>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly ILogger<GetExplorerProfileByUserIdQueryHandler> _logger;

        public GetExplorerProfileByUserIdQueryHandler(
            IGenericRepository<ExplorerProfile> repository,
            ILogger<GetExplorerProfileByUserIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetExplorerDto>> Handle(GetExplorerProfileByUserIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching explorer profile for user {UserId}", request.UserId);

            var profile = await _repository.GetTable()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile is null)
            {
                _logger.LogInformation("Failed to find explorer profile for user {UserId}", request.UserId);
                return ApiResponse<GetExplorerDto>.Failure(ErrorCode.NotFound);
            }

            var profileDto = ExplorerProfileMapper.ToGetDto(profile);

            return ApiResponse<GetExplorerDto>.Success(profileDto);
        }
    }
}
