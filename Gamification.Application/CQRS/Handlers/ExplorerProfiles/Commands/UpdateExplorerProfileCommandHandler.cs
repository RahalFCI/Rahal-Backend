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
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Commands
{
    public class UpdateExplorerProfileCommandHandler : IRequestHandler<UpdateExplorerProfileCommand, ApiResponse<GetExplorerDto>>
    {
        private readonly IGamificationRepository<ExplorerProfile> _repository;
        private readonly ILogger<UpdateExplorerProfileCommandHandler> _logger;

        public UpdateExplorerProfileCommandHandler(IGamificationRepository<ExplorerProfile> repository, ILogger<UpdateExplorerProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetExplorerDto>> Handle(UpdateExplorerProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Updating explorer profile for user {UserId}", request.UpdateExplorerDto.UserId);

                var existingExplorer = await _repository.GetTable().Where(x => x.UserId == request.UpdateExplorerDto.UserId).FirstOrDefaultAsync(cancellationToken);
                if (existingExplorer is null)
                {
                    _logger.LogError("Explorer profile not found for user {UserId}", request.UpdateExplorerDto.UserId);
                    return ApiResponse<GetExplorerDto>.Failure(ErrorCode.NotFound);
                }

                existingExplorer.DisplayName = request.UpdateExplorerDto.DisplayName;
                existingExplorer.ProfilePictureURL = request.UpdateExplorerDto.ProfilePictureUrl;
                existingExplorer.BirthDate = request.UpdateExplorerDto.BirthDate;
                existingExplorer.Gender = request.UpdateExplorerDto.Gender;
                existingExplorer.Bio = request.UpdateExplorerDto.Bio;
                existingExplorer.CountryCode = request.UpdateExplorerDto.CountryCode;
                existingExplorer.IsPublic = request.UpdateExplorerDto.IsPublic;
                existingExplorer.IsPremium = request.UpdateExplorerDto.IsPremium;
                existingExplorer.Level = request.UpdateExplorerDto.Level;

                _repository.Update(existingExplorer);
                await _repository.SaveChangesAsync();

                _logger.LogError("Updated explorer profile for user {UserId}", request.UpdateExplorerDto.UserId);

                return ApiResponse<GetExplorerDto>.Success(ExplorerProfileMapper.ToGetDto(existingExplorer));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating explorer profile");
                return ApiResponse<GetExplorerDto>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
