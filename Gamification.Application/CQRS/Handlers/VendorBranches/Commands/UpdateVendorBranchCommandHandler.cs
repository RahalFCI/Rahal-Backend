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
    public class UpdateVendorBranchCommandHandler
        : IRequestHandler<UpdateVendorBranchCommand, ApiResponse<VendorBranchDto>>
    {
        private readonly IGamificationRepository<VendorPlace> _repository;
        private readonly ILogger<UpdateVendorBranchCommandHandler> _logger;

        public UpdateVendorBranchCommandHandler(
            IGamificationRepository<VendorPlace> repository,
            ILogger<UpdateVendorBranchCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<VendorBranchDto>> Handle(UpdateVendorBranchCommand request, CancellationToken cancellationToken)
        {
            var vendorPlace = await _repository.GetTable()
                .FirstOrDefaultAsync(vp => vp.Id == request.BranchId && vp.VendorId == request.VendorId, cancellationToken);

            if (vendorPlace is null)
            {
                return ApiResponse<VendorBranchDto>.Failure(ErrorCode.NotFound);
            }

            if (request.Dto.IsPrimary)
            {
                var primaryBranches = await _repository.GetTable()
                    .Where(vp => vp.VendorId == request.VendorId && vp.Id != request.BranchId && vp.IsPrimary)
                    .ToListAsync(cancellationToken);

                foreach (var branch in primaryBranches)
                {
                    branch.IsPrimary = false;
                }
            }

            vendorPlace.BranchName = request.Dto.BranchName;
            vendorPlace.PhoneNumber = request.Dto.PhoneNumber;
            vendorPlace.Notes = request.Dto.Notes;
            vendorPlace.IsPrimary = request.Dto.IsPrimary;
            vendorPlace.IsActive = request.Dto.IsActive;

            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated vendor branch {BranchId} for vendor {VendorId}", request.BranchId, request.VendorId);

            return ApiResponse<VendorBranchDto>.Success(VendorPlaceMapper.ToDto(vendorPlace));
        }
    }
}
