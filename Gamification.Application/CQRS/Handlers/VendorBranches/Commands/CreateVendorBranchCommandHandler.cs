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
        : IRequestHandler<CreateVendorBranchCommand, ApiResponse<VendorBranchDto>>
    {
        private readonly IGamificationRepository<VendorPlace> _vendorPlaceRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateVendorBranchCommandHandler> _logger;

        public CreateVendorBranchCommandHandler(
            IGamificationRepository<VendorPlace> vendorPlaceRepository,
            IMediator mediator,
            ILogger<CreateVendorBranchCommandHandler> logger)
        {
            _vendorPlaceRepository = vendorPlaceRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<VendorBranchDto>> Handle(CreateVendorBranchCommand request, CancellationToken cancellationToken)
        {
            var vendorExists = await _mediator.Send(new GetVendorProfileByIdQuery(request.VendorId), cancellationToken);

            if (!vendorExists.IsSuccess)
            {
                return ApiResponse<VendorBranchDto>.Failure(ErrorCode.NotFound);
            }

            var placeAlreadyLinked = await _vendorPlaceRepository.GetTable()
                .AnyAsync(vp => vp.PlaceId == request.PlaceId, cancellationToken);

            if (placeAlreadyLinked)
            {
                return ApiResponse<VendorBranchDto>.Failure(ErrorCode.AlreadyExists);
            }

            if (request.Dto.IsPrimary)
            {
                await ClearPrimaryBranchesAsync(request.VendorId, cancellationToken);
            }

            var vendorPlace = VendorPlaceMapper.ToEntity(request.VendorId, request.PlaceId, request.Dto);
            _vendorPlaceRepository.Add(vendorPlace);
            await _vendorPlaceRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created vendor branch {BranchId} for vendor {VendorId} and place {PlaceId}",
                vendorPlace.Id, request.VendorId, request.PlaceId);

            return ApiResponse<VendorBranchDto>.Success(VendorPlaceMapper.ToDto(vendorPlace));
        }

        private async Task ClearPrimaryBranchesAsync(Guid vendorId, CancellationToken cancellationToken)
        {
            var primaryBranches = await _vendorPlaceRepository.GetTable()
                .Where(vp => vp.VendorId == vendorId && vp.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var branch in primaryBranches)
            {
                branch.IsPrimary = false;
            }
        }
    }
}
