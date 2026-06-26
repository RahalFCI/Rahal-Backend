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
        : IRequestHandler<UpdateVendorBranchCommand, ApiResponse<GetVendorBranchDto>>
    {
        private readonly IGamificationRepository<VendorBranch> _repository;
        private readonly IVendorBranchPlaceClient _placeClient;
        private readonly ILogger<UpdateVendorBranchCommandHandler> _logger;

        public UpdateVendorBranchCommandHandler(
            IGamificationRepository<VendorBranch> repository,
            IVendorBranchPlaceClient placeClient,
            ILogger<UpdateVendorBranchCommandHandler> logger)
        {
            _repository = repository;
            _placeClient = placeClient;
            _logger = logger;
        }

        public async Task<ApiResponse<GetVendorBranchDto>> Handle(UpdateVendorBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _repository.GetTable()
                .FirstOrDefaultAsync(vb => vb.Id == request.BranchId, cancellationToken);

            if (branch is null)
            {
                return ApiResponse<GetVendorBranchDto>.Failure(ErrorCode.NotFound);
            }

            var placeResult = await _placeClient.UpdatePlaceAsync(branch.PlaceId, request.Dto, cancellationToken);
            if (!placeResult.IsSuccess)
            {
                return ApiResponse<GetVendorBranchDto>.Failure(placeResult.errorCode);
            }

            branch.BranchName = request.Dto.BranchName;
            branch.PhoneNumber = request.Dto.PhoneNumber;
            branch.Notes = request.Dto.Notes;
            branch.IsActive = request.Dto.IsActive;

            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated vendor branch {BranchId}", branch.Id);

            return ApiResponse<GetVendorBranchDto>.Success(VendorBranchMapper.ToGetDto(branch, placeResult.Data));
        }
    }
}
