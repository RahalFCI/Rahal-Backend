using Gamification.Application.CQRS.Commands.VendorBranches;
using Gamification.Application.DTOs.VendorBranches;
using Gamification.Application.Interfaces;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.VendorBranches.Commands
{
    public class DeleteVendorBranchCommandHandler
        : IRequestHandler<DeleteVendorBranchCommand, ApiResponse<VendorBranchDto>>
    {
        private readonly IGamificationRepository<VendorPlace> _repository;
        private readonly ILogger<DeleteVendorBranchCommandHandler> _logger;

        public DeleteVendorBranchCommandHandler(
            IGamificationRepository<VendorPlace> repository,
            ILogger<DeleteVendorBranchCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<VendorBranchDto>> Handle(DeleteVendorBranchCommand request, CancellationToken cancellationToken)
        {
            var vendorPlace = await _repository.GetTable()
                .FirstOrDefaultAsync(vp => vp.Id == request.BranchId && vp.VendorId == request.VendorId, cancellationToken);

            if (vendorPlace is null)
            {
                return ApiResponse<VendorBranchDto>.Failure(ErrorCode.NotFound);
            }

            vendorPlace.IsActive = false;
            vendorPlace.IsDeleted = true;
            vendorPlace.DeletedAt = DateTime.UtcNow;

            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted vendor branch {BranchId} for vendor {VendorId}", request.BranchId, request.VendorId);

            return ApiResponse<VendorBranchDto>.Success(VendorPlaceMapper.ToDto(vendorPlace));
        }
    }
}
