using Microsoft.EntityFrameworkCore;
using Rewards.Application.DTOs.PlanTiers;
using Rewards.Application.Interfaces;
using Rewards.Application.Mappers;
using Rewards.Domain.Entities;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Domain.Enums;
using Shared.Infrastructure.Pagination;

namespace Rewards.Application.Services
{
    internal class PlanTierService : IPlanTierService
    {
        private readonly IRewardsRepository<PlanTier> _repository;

        public PlanTierService(IRewardsRepository<PlanTier> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<GetPlanTierDto>> CreateAsync(CreatePlanTierDto dto, CancellationToken cancellationToken = default)
        {
            var exists = await _repository.GetTable().AnyAsync(p => p.Name == dto.Name, cancellationToken);
            if (exists)
                return ApiResponse<GetPlanTierDto>.Failure(ErrorCode.AlreadyExists);

            var planTier = RewardsMapper.ToEntity(dto);
            _repository.Add(planTier);
            await _repository.SaveChangesAsync(cancellationToken);
            return ApiResponse<GetPlanTierDto>.Success(RewardsMapper.ToDto(planTier));
        }

        public async Task<ApiResponse<GetPlanTierDto>> UpdateAsync(Guid id, UpdatePlanTierDto dto, CancellationToken cancellationToken = default)
        {
            var planTier = await _repository.GetByIdAsync(id, cancellationToken);
            if (planTier is null)
                return ApiResponse<GetPlanTierDto>.Failure(ErrorCode.NotFound);

            var duplicateName = await _repository.GetTable()
                .AnyAsync(p => p.Name == dto.Name && p.Id != id, cancellationToken);
            if (duplicateName)
                return ApiResponse<GetPlanTierDto>.Failure(ErrorCode.Conflict);

            RewardsMapper.Update(planTier, dto);
            await _repository.SaveChangesAsync(cancellationToken);
            return ApiResponse<GetPlanTierDto>.Success(RewardsMapper.ToDto(planTier));
        }

        public async Task<ApiResponse<GetPlanTierDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var planTier = await _repository.GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return planTier is null
                ? ApiResponse<GetPlanTierDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<GetPlanTierDto>.Success(RewardsMapper.ToDto(planTier));
        }

        public async Task<ApiResponse<PagedResult<GetPlanTierDto>>> GetAllAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var query = _repository.GetTable()
                .AsNoTracking()
                .OrderBy(p => p.WeeklyPrice)
                .Select(p => RewardsMapper.ToDto(p));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetPlanTierDto>>.Success(result);
        }
    }
}
