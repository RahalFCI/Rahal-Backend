using Gamification.Application.CQRS.Commands.VendorBranches;
using Gamification.Application.CQRS.Queries.VendorProfiles;
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
    public class CreateVendorBranchCommandHandler
        : IRequestHandler<CreateVendorBranchCommand, ApiResponse<GetVendorBranchDto>>
    {
        private readonly IGamificationRepository<VendorBranch> _repository;
        private readonly IVendorBranchPlaceClient _placeClient;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateVendorBranchCommandHandler> _logger;

        public CreateVendorBranchCommandHandler(
            IGamificationRepository<VendorBranch> repository,
            IVendorBranchPlaceClient placeClient,
            IMediator mediator,
            ILogger<CreateVendorBranchCommandHandler> logger)
        {
            _repository = repository;
            _placeClient = placeClient;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<GetVendorBranchDto>> Handle(CreateVendorBranchCommand request, CancellationToken cancellationToken)
        {
            var vendor = await _mediator.Send(new GetVendorProfileByIdQuery(request.Dto.VendorId), cancellationToken);
            if (!vendor.IsSuccess)
            {
                return ApiResponse<GetVendorBranchDto>.Failure(ErrorCode.NotFound);
            }

            var placeResult = await _placeClient.CreatePlaceAsync(request.Dto, cancellationToken);
            if (!placeResult.IsSuccess)
            {
                return ApiResponse<GetVendorBranchDto>.Failure(placeResult.errorCode);
            }

            var branch = VendorBranchMapper.ToEntity(placeResult.Data.PlaceId, request.Dto);

            try
            {
                _repository.Add(branch);
                await _repository.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save vendor branch for vendor {VendorId}; compensating place {PlaceId}",
                    request.Dto.VendorId,
                    placeResult.Data.PlaceId);
                await _placeClient.DeletePlaceAsync(placeResult.Data.PlaceId, cancellationToken);
                return ApiResponse<GetVendorBranchDto>.Failure(ErrorCode.DatabaseError);
            }

            _logger.LogInformation("Created vendor branch {BranchId} for vendor {VendorId} and place {PlaceId}",
                branch.Id,
                branch.VendorId,
                branch.PlaceId);

            return ApiResponse<GetVendorBranchDto>.Success(VendorBranchMapper.ToGetDto(branch, placeResult.Data));
        }
    }
}
