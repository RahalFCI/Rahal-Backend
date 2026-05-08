using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.XpTransactions;
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
    public class PermenantDeleteExplorerProfileCommandHandler : IRequestHandler<PermenantDeleteExplorerProfileCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly ILogger<PermenantDeleteExplorerProfileCommandHandler> _logger;

        public PermenantDeleteExplorerProfileCommandHandler(IGenericRepository<ExplorerProfile> repository, ILogger<PermenantDeleteExplorerProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(PermenantDeleteExplorerProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Deleting explorer profile for user {UserId}", request.Id);

                var Explorer = await _repository.GetTable().Where(x => x.UserId == request.Id).FirstOrDefaultAsync(cancellationToken);
                if (Explorer is null)
                {
                    _logger.LogError("Explorer profile does not exist for user {UserId}", request.Id);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }
           
                _repository.Delete(Explorer);
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
