using Gamification.Application.CQRS.Commands.VendorBranches;
using Gamification.Application.Interfaces;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.VendorBranches.Commands
{
    public class DeleteVendorBranchCommandHandler
        : IRequestHandler<DeleteVendorBranchCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<VendorBranch> _repository;
        private readonly IVendorBranchPlaceClient _placeClient;
        private readonly ILogger<DeleteVendorBranchCommandHandler> _logger;

        public DeleteVendorBranchCommandHandler(
            IGamificationRepository<VendorBranch> repository,
            IVendorBranchPlaceClient placeClient,
            ILogger<DeleteVendorBranchCommandHandler> logger)
        {
            _repository = repository;
            _placeClient = placeClient;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteVendorBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _repository.GetTable()
                .FirstOrDefaultAsync(vb => vb.Id == request.BranchId, cancellationToken);

            if (branch is null)
            {
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            branch.IsActive = false;
            branch.IsDeleted = true;
            branch.DeletedAt = DateTime.UtcNow;

            await _repository.SaveChangesAsync(cancellationToken);

            var placeResult = await _placeClient.DeletePlaceAsync(branch.PlaceId, cancellationToken);
            if (!placeResult.IsSuccess)
            {
                _logger.LogWarning("Vendor branch {BranchId} was deleted, but linked place {PlaceId} delete failed with {ErrorCode}",
                    branch.Id,
                    branch.PlaceId,
                    placeResult.errorCode);
                return ApiResponse<string>.Failure(placeResult.errorCode);
            }

            _logger.LogInformation("Deleted vendor branch {BranchId}", branch.Id);

            return ApiResponse<string>.Success("Vendor branch deleted successfully");
        }
    }
}
