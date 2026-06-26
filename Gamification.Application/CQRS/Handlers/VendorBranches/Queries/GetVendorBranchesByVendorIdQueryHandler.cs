using Gamification.Application.CQRS.Queries.VendorBranches;
using Gamification.Application.DTOs.VendorBranches;
using Gamification.Application.Interfaces;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;

namespace Gamification.Application.CQRS.Handlers.VendorBranches.Queries
{
    public class GetVendorBranchesByVendorIdQueryHandler
        : IRequestHandler<GetVendorBranchesByVendorIdQuery, ApiResponse<PagedResult<GetVendorBranchDto>>>
    {
        private readonly IGamificationRepository<VendorBranch> _repository;
        private readonly IVendorBranchPlaceClient _placeClient;

        public GetVendorBranchesByVendorIdQueryHandler(
            IGamificationRepository<VendorBranch> repository,
            IVendorBranchPlaceClient placeClient)
        {
            _repository = repository;
            _placeClient = placeClient;
        }

        public async Task<ApiResponse<PagedResult<GetVendorBranchDto>>> Handle(GetVendorBranchesByVendorIdQuery request, CancellationToken cancellationToken)
        {
            var pagedBranches = await _repository.GetTable()
                .AsNoTracking()
                .Where(vb => vb.VendorId == request.VendorId)
                .OrderBy(vb => vb.BranchName)
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            var branchList = pagedBranches.Items.ToList();
            var placesResult = await _placeClient.GetPlacesAsync(branchList.Select(vb => vb.PlaceId), cancellationToken);
            if (!placesResult.IsSuccess)
            {
                return ApiResponse<PagedResult<GetVendorBranchDto>>.Failure(placesResult.errorCode);
            }

            var placeById = placesResult.Data.ToDictionary(p => p.PlaceId);
            var items = branchList
                .Where(vb => placeById.ContainsKey(vb.PlaceId))
                .Select(vb => VendorBranchMapper.ToGetDto(vb, placeById[vb.PlaceId]))
                .ToList();

            return ApiResponse<PagedResult<GetVendorBranchDto>>.Success(new PagedResult<GetVendorBranchDto>
            {
                Items = items,
                TotalCount = pagedBranches.TotalCount,
                Page = pagedBranches.Page,
                PageSize = pagedBranches.PageSize
            });
        }
    }
}
