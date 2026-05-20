using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.UserStat.Commands
{
    public class CreateUserStatsCommandHandler : IRequestHandler<CreateUserStatsCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.UserStats> _repository;
        private readonly ILogger<CreateUserStatsCommandHandler> _logger;

        public CreateUserStatsCommandHandler(
            IGenericRepository<Domain.Entities.UserStats> repository,
            ILogger<CreateUserStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(CreateUserStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating user stats for explorer {ExplorerId}", request.Dto.ExplorerId);

            var existingStats = await _repository.GetTable().Where(us => us.ExplorerProfileId == request.Dto.ExplorerId).AnyAsync(cancellationToken);
            if(existingStats)
            {
                _logger.LogInformation("User stats for explorer {ExplorerId} already exists", request.Dto.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.AlreadyExists);
            }

            var userStats = UserStatsMapper.ToEntity(request.Dto);
            _repository.Add(userStats);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User stats {UserStatsId} created successfully for explorer {ExplorerId}",
                userStats.Id, request.Dto.ExplorerId);

            return ApiResponse<string>.Success($"User stats created successfully. ID: {userStats.Id}");
        }
    }
}
