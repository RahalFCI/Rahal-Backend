using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.DTOs.Explorer;
using Gamification.Application.Interfaces;
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
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

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

                var explorerProfile = await _repository.GetTable().Where(x => x.UserId == request.UpdateExplorerDto.UserId).FirstOrDefaultAsync(cancellationToken);
                if (explorerProfile is null)
                {
                    _logger.LogError("Explorer profile already exists for user {UserId}", request.UpdateExplorerDto.UserId);
                    return ApiResponse<GetExplorerDto>.Failure(ErrorCode.NotFound);
                }

                explorerProfile.DisplayName = request.UpdateExplorerDto.DisplayName;
                explorerProfile.BirthDate = request.UpdateExplorerDto.BirthDate;
                explorerProfile.Gender = request.UpdateExplorerDto.Gender;
                explorerProfile.Bio = request.UpdateExplorerDto.Bio;
                explorerProfile.CountryCode = request.UpdateExplorerDto.CountryCode;
                explorerProfile.IsPublic = request.UpdateExplorerDto.IsPublic;
                explorerProfile.IsPremium = request.UpdateExplorerDto.IsPremium;
                explorerProfile.Level = request.UpdateExplorerDto.Level;

                _repository.Update(explorerProfile);
                await _repository.SaveChangesAsync();

                _logger.LogError("Updated explorer profile for user {UserId}", request.UpdateExplorerDto.UserId);

                return ApiResponse<GetExplorerDto>.Success(ExplorerProfileMapper.ToGetDto(explorerProfile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating explorer profile");
                return ApiResponse<GetExplorerDto>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
