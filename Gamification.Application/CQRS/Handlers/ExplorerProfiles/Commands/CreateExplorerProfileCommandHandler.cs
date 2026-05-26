using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.DTOs.Explorer;
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

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Commands
{
    public class CreateExplorerProfileCommandHandler : IRequestHandler<CreateExplorerProfileCommand, ApiResponse<GetExplorerDto>>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly ILogger<CreateExplorerProfileCommandHandler> _logger;

        public CreateExplorerProfileCommandHandler(IGenericRepository<ExplorerProfile> repository, ILogger<CreateExplorerProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetExplorerDto>> Handle(CreateExplorerProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Creating explorer profile for user {UserId}", request.ExplorerProfileDto.UserId);
                var explorerProfile = ExplorerProfileMapper.ToEntity(request.ExplorerProfileDto);

                var existingExplorer = await _repository.GetTable().Where(x => x.UserId == request.ExplorerProfileDto.UserId).AnyAsync(cancellationToken);
                if (!existingExplorer)
                {
                    _logger.LogError("Explorer profile already exists for user {UserId}", request.ExplorerProfileDto.UserId);
                    return ApiResponse<GetExplorerDto>.Failure(ErrorCode.AlreadyExists);
                }

                explorerProfile.ProfilePictureURL = request.ProfilePictureUrl;

                _repository.Add(explorerProfile);
                await _repository.SaveChangesAsync();

                _logger.LogError("Created explorer profile for user {UserId}", request.ExplorerProfileDto.UserId);

                var dto = ExplorerProfileMapper.ToGetDto(explorerProfile);
                return ApiResponse<GetExplorerDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating explorer profile");
                return ApiResponse<GetExplorerDto>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
