using Gamification.Application.CQRS.Queries.ExplorerProfiles;
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
using Users.Application.DTOs.Explorer;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries
{
    public class GetExplorerProfileByIdQueryHandler : IRequestHandler<GetExplorerProfileByIdQuery, ApiResponse<GetExplorerDto>>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly ILogger<GetExplorerProfileByIdQueryHandler> _logger;

        public GetExplorerProfileByIdQueryHandler(
            IGenericRepository<ExplorerProfile> repository,
            ILogger<GetExplorerProfileByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<ApiResponse<GetExplorerDto>> Handle(GetExplorerProfileByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching explorerProfile {ExplorerId}", request.Id);

            var explorerProfile = await _repository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
            if(explorerProfile is null)
            {
                _logger.LogInformation("Failed to find explorerProfile {ExplorerId}", request.Id);
                return ApiResponse<GetExplorerDto>.Failure(ErrorCode.NotFound);
            }
            var dto = ExplorerProfileMapper.ToGetDto(explorerProfile);

            _logger.LogInformation("Retrieved explorerProfile {ExplorerId}",request.Id );

            return ApiResponse<GetExplorerDto>.Success(dto);
        }
    }
}
