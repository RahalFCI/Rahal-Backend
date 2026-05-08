using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.XpTransactions;
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
using Users.Application.DTOs.Explorer;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Commands
{
    public class DeleteExplorerProfileCommandHandler : IRequestHandler<DeleteXpTransactionCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly ILogger<DeleteExplorerProfileCommandHandler> _logger;

        public DeleteExplorerProfileCommandHandler(IGenericRepository<ExplorerProfile> repository, ILogger<DeleteExplorerProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteXpTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Deleting explorer profile for user {UserId}", request.Id);

                var existingExplorer = await _repository.GetTable().Where(x => x.UserId == request.Id).AnyAsync(cancellationToken);
                if (!existingExplorer)
                {
                    _logger.LogError("Explorer profile does not exist for user {UserId}", request.Id);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                ExplorerProfile explorerProfile = new ExplorerProfile { 
                    UserId = request.Id,
                    IsDeleted = true
                };

                _repository.SaveInclude(explorerProfile, nameof(explorerProfile.IsDeleted));
                await _repository.SaveChangesAsync();

                _logger.LogError("Deleted explorer profile for user {UserId}", request.Id);

                return ApiResponse<string>.Success("Explorer profile deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting explorer profile");
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
