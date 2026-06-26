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
        : IRequestHandler<GetVendorBranchesByVendorIdQuery, ApiResponse<PagedResult<VendorBranchDto>>>
    {
        private readonly IGamificationRepository<VendorPlace> _repository;

        public GetVendorBranchesByVendorIdQueryHandler(IGamificationRepository<VendorPlace> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResult<VendorBranchDto>>> Handle(GetVendorBranchesByVendorIdQuery request, CancellationToken cancellationToken)
        {
            var branches = _repository.GetTable()
                .AsNoTracking()
                .Where(vp => vp.VendorId == request.VendorId)
                .OrderByDescending(vp => vp.IsPrimary)
                .ThenBy(vp => vp.BranchName)
                .Select(vp => VendorPlaceMapper.ToDto(vp));

            var response = await PaginationExtensions.ToPagedResultAsync(branches, request.PaginationRequest, cancellationToken);

            return ApiResponse<PagedResult<VendorBranchDto>>.Success(response);
        }
    }
}
