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
        : IRequestHandler<GetVendorBranchByIdQuery, ApiResponse<VendorBranchDto>>
    {
        private readonly IGamificationRepository<VendorPlace> _repository;

        public GetVendorBranchByIdQueryHandler(IGamificationRepository<VendorPlace> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<VendorBranchDto>> Handle(GetVendorBranchByIdQuery request, CancellationToken cancellationToken)
        {
            var branch = await _repository.GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(vp => vp.Id == request.BranchId && vp.VendorId == request.VendorId, cancellationToken);

            return branch is null
                ? ApiResponse<VendorBranchDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<VendorBranchDto>.Success(VendorPlaceMapper.ToDto(branch));
        }
    }
}
