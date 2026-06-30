using Gamification.Application.CQRS.Queries.VendorBranches;
using Gamification.Application.DTOs.VendorBranches;
using Gamification.Application.Interfaces;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.VendorBranches.Queries
{
    public class GetVendorBranchByIdQueryHandler
        : IRequestHandler<GetVendorBranchByIdQuery, ApiResponse<GetVendorBranchDto>>
    {
        private readonly IGamificationRepository<VendorBranch> _repository;
        private readonly IVendorBranchPlaceClient _placeClient;

        public GetVendorBranchByIdQueryHandler(
            IGamificationRepository<VendorBranch> repository,
            IVendorBranchPlaceClient placeClient)
        {
            _repository = repository;
            _placeClient = placeClient;
        }

        public async Task<ApiResponse<GetVendorBranchDto>> Handle(GetVendorBranchByIdQuery request, CancellationToken cancellationToken)
        {
            var branch = await _repository.GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(vb => vb.Id == request.BranchId, cancellationToken);

            if (branch is null)
            {
                return ApiResponse<GetVendorBranchDto>.Failure(ErrorCode.NotFound);
            }

            var placeResult = await _placeClient.GetPlaceAsync(branch.PlaceId, cancellationToken);
            return placeResult.IsSuccess
                ? ApiResponse<GetVendorBranchDto>.Success(VendorBranchMapper.ToGetDto(branch, placeResult.Data))
                : ApiResponse<GetVendorBranchDto>.Failure(placeResult.errorCode);
        }
    }
}
