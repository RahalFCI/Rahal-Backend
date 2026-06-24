using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PlanTierService> _logger;

        public PlanTierService(
            IRewardsRepository<PlanTier> repository,
            ILogger<PlanTierService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting plan tier {PlanTierId}", id);

            var planTier = await _repository.GetByIdAsync(id, cancellationToken);
            if (planTier is null)
            {
                _logger.LogWarning("Plan tier {PlanTierId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            planTier.IsDeleted = true;
            planTier.DeletedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Plan tier {PlanTierId} deleted successfully", id);
            
            return ApiResponse<string>.Success("Plan tier deleted successfully.");
        }
        
        public async Task<ApiResponse<string>> PermanentDeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Permanently deleting plan tier {PlanTierId}", id);

            var planTier = await _repository.GetTable().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (planTier is null)
            {
                _logger.LogWarning("Plan tier {PlanTierId} not found for permanent deletion", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            _repository.Delete(planTier);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Plan tier {PlanTierId} permanently deleted", id);
            
            return ApiResponse<string>.Success("Plan tier deleted successfully.");
        }

        public async Task<ApiResponse<GetPlanTierDto>> CreateAsync(CreatePlanTierDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating plan tier {PlanTierName}", dto.Name);

            var exists = await _repository.GetTable().AnyAsync(p => p.Name == dto.Name, cancellationToken);
            if (exists)
            {
                _logger.LogWarning("Plan tier {PlanTierName} already exists", dto.Name);
                return ApiResponse<GetPlanTierDto>.Failure(ErrorCode.AlreadyExists);
            }

            var planTier = RewardsMapper.ToEntity(dto);
            _repository.Add(planTier);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Plan tier {PlanTierId} created successfully", planTier.Id);

            return ApiResponse<GetPlanTierDto>.Success(RewardsMapper.ToDto(planTier));
        }

        public async Task<ApiResponse<GetPlanTierDto>> UpdateAsync(Guid id, UpdatePlanTierDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating plan tier {PlanTierId}", id);

            var planTier = await _repository.GetByIdAsync(id, cancellationToken);
            if (planTier is null)
            {
                _logger.LogWarning("Plan tier {PlanTierId} not found", id);
                return ApiResponse<GetPlanTierDto>.Failure(ErrorCode.NotFound);
            }

            var duplicateName = await _repository.GetTable()
                .AnyAsync(p => p.Name == dto.Name && p.Id != id, cancellationToken);
            if (duplicateName)
            {
                _logger.LogWarning("Plan tier update rejected for {PlanTierId}. Duplicate name {PlanTierName}", id, dto.Name);
                return ApiResponse<GetPlanTierDto>.Failure(ErrorCode.Conflict);
            }

            RewardsMapper.Update(planTier, dto);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Plan tier {PlanTierId} updated successfully", id);

            return ApiResponse<GetPlanTierDto>.Success(RewardsMapper.ToDto(planTier));
        }

        public async Task<ApiResponse<GetPlanTierDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching plan tier {PlanTierId}", id);

            var planTier = await _repository.GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (planTier is null)
                _logger.LogWarning("Plan tier {PlanTierId} not found", id);

            return planTier is null
                ? ApiResponse<GetPlanTierDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<GetPlanTierDto>.Success(RewardsMapper.ToDto(planTier));
        }

        public async Task<ApiResponse<PagedResult<GetPlanTierDto>>> GetAllAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all plan tiers - page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

            var query = _repository.GetTable()
                .AsNoTracking()
                .OrderBy(p => p.WeeklyPrice)
                .Select(p => RewardsMapper.ToDto(p));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetPlanTierDto>>.Success(result);
        }
    }
}
