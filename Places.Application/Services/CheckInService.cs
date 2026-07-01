using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Application.DTOs.CheckIn;
using Places.Application.Factories;
using Places.Application.Interfaces;
using Places.Application.Mappers;
using Places.Domain.Entities;
using Shared.Application.DTOs;
using Shared.Application.Events.CheckIns;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Domain.Enums;
using Shared.Infrastructure.Pagination;

namespace Places.Application.Services
{
    internal class CheckInService : ICheckInService
    {
        private readonly IPlacesRepository<CheckIn> _checkInRepository;
        private readonly IPlacesRepository<Place> _placeRepository;
        private readonly ICheckInValidatorService _validator;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<CheckInService> _logger;

        public CheckInService(
            IPlacesRepository<CheckIn> checkInRepository,
            IPlacesRepository<Place> placeRepository,
            ICheckInValidatorService validator,
            IPublishEndpoint publisher,
            ILogger<CheckInService> logger)
        {
            _checkInRepository = checkInRepository;
            _placeRepository = placeRepository;
            _validator = validator;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<ApiResponse<GetCheckInDto>> GetCheckInAsync(Guid explorerId, Guid placeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching check-in for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);

            var checkIn = await _checkInRepository.GetTable()
                .AsNoTracking()
                .Include(c => c.Place)
                .FirstOrDefaultAsync(c => c.ExplorerId == explorerId && c.PlaceId == placeId, cancellationToken);

            if (checkIn is null)
            {
                _logger.LogWarning("Check-in not found for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);
                return ApiResponse<GetCheckInDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<GetCheckInDto>.Success(CheckInMapper.ToGetDto(checkIn));
        }

        public async Task<ApiResponse<PagedResult<GetCheckInDto>>> GetAllCheckInAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all check-ins - page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

            var checkIns = await _checkInRepository.GetTable()
                .AsNoTracking()
                .Select(c => CheckInMapper.ToGetDto(c))
                .ToPagedResultAsync(request, cancellationToken);

            return ApiResponse<PagedResult<GetCheckInDto>>.Success(checkIns);
        }

        public async Task<ApiResponse<PagedResult<GetCheckInDto>>> GetCheckInsByPlaceIdAsync(Guid placeId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching check-ins for place {PlaceId} - page {Page}, pageSize {PageSize}", placeId, request.Page, request.PageSize);

            var place = await _placeRepository.GetByIdAsync(placeId, cancellationToken);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found", placeId);
                return ApiResponse<PagedResult<GetCheckInDto>>.Failure(ErrorCode.NotFound);
            }

            var checkIns = await _checkInRepository.GetTable()
                .AsNoTracking()
                .Where(c => c.PlaceId == placeId)
                .Select(c => CheckInMapper.ToGetDto(c))
                .ToPagedResultAsync(request, cancellationToken);

            return ApiResponse<PagedResult<GetCheckInDto>>.Success(checkIns);
        }

        public async Task<ApiResponse<PagedResult<GetCheckInDto>>> GetCheckInsByExplorerIdAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching check-ins for explorer {ExplorerId} - page {Page}, pageSize {PageSize}", explorerId, request.Page, request.PageSize);

            var checkIns = await _checkInRepository.GetTable()
                .AsNoTracking()
                .Where(c => c.ExplorerId == explorerId)
                .Select(c => CheckInMapper.ToGetDto(c))
                .ToPagedResultAsync(request, cancellationToken);

            return ApiResponse<PagedResult<GetCheckInDto>>.Success(checkIns);
        }

        public async Task<ApiResponse<string>> CheckInAsync(
            Guid explorerId,
            CheckInRequestDto request,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Processing check-in request for explorer {ExplorerId} at place {PlaceId}", 
                explorerId, request.PlaceId);

            var place = await _placeRepository.GetTable().Include(p => p.PlaceCategory).FirstOrDefaultAsync(p => p.Id == request.PlaceId, ct);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found for check-in", request.PlaceId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (place.PlaceCategoryId == Guid.Parse("c6666666-6666-6666-6666-666666666666") || place.PlaceCategory?.Name == "Vendor")
            {
                _logger.LogWarning("Check-in attempt to a restricted place category for explorer {ExplorerId} at place {PlaceId}", 
                    explorerId, request.PlaceId);
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }

            var validationResult = await _validator.ValidateAsync(
                request, explorerId, place, ct);

            var checkIn = validationResult.IsValid
                ? CheckInFactory.CreateVerified(
                    explorerId, request.PlaceId,
                    request.Latitude, request.Longitude,
                    validationResult.RiskScore)
                : CheckInFactory.CreateFailed(
                    explorerId, request.PlaceId,
                    request.Latitude, request.Longitude,
                    validationResult.RiskScore);

            _checkInRepository.Add(checkIn);

            await _checkInRepository.SaveChangesAsync(ct);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Check-in validation failed for explorer {ExplorerId}: errorCode={ErrorCode}", 
                    explorerId, validationResult.ErrorCode);
                return ApiResponse<string>.Failure(validationResult.ErrorCode!.Value);
            }

            _logger.LogInformation("Check-in successful for explorer {ExplorerId}", explorerId);

            try
            {
                await _publisher.Publish(new CreateCheckInEvent(explorerId, checkIn.Id), ct);

                _logger.LogInformation("Check-in event published for explorer {ExplorerId} with check-in {CheckInId}",
                    explorerId, checkIn.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish check-in event for explorer {ExplorerId} with check-in {CheckInId}",
                    explorerId, checkIn.Id);
            }

            return ApiResponse<string>.Success("Checked in successfully");
        }

        public async Task<ApiResponse<string>> UpdateCheckInStatusAsync(Guid explorerId, Guid placeId, UpdateCheckInDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating check-in status for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);

            var checkIn = await _checkInRepository.GetTable()
                .FirstOrDefaultAsync(c => c.ExplorerId == explorerId && c.PlaceId == placeId, cancellationToken);

            if (checkIn is null)
            {
                _logger.LogWarning("Check-in not found for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            CheckInMapper.UpdateEntity(checkIn, dto);
            await _checkInRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in status updated successfully for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);

            return ApiResponse<string>.Success("Check-in status updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteCheckInAsync(Guid explorerId, Guid placeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting check-in for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);

            var checkIn = await _checkInRepository.GetTable()
                .FirstOrDefaultAsync(c => c.ExplorerId == explorerId && c.PlaceId == placeId, cancellationToken);

            if (checkIn is null)
            {
                _logger.LogWarning("Check-in not found for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            checkIn.IsDeleted = true;
            await _checkInRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in deleted successfully for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);

            return ApiResponse<string>.Success("Check-in deleted successfully");
        }

        public async Task<ApiResponse<string>> DeleteCheckInPermanentlyAsync(Guid explorerId, Guid placeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Permanently deleting check-in for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);

            var checkIn = await _checkInRepository.GetTable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.ExplorerId == explorerId && c.PlaceId == placeId, cancellationToken);

            if (checkIn is null)
            {
                _logger.LogWarning("Check-in not found for permanent deletion: explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            _checkInRepository.Delete(checkIn);
            await _checkInRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Check-in permanently deleted for explorer {ExplorerId} at place {PlaceId}", explorerId, placeId);

            return ApiResponse<string>.Success("Check-in permanently deleted");
        }

        public async Task<ApiResponse<PagedResult<GetCheckInDto>>> GetPendingCheckInsAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching pending check-ins - page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

            var checkIns = await _checkInRepository.GetTable()
                .AsNoTracking()
                .Where(c => c.ValidationStatus == Places.Domain.Enums.CheckInValidationStatus.Pending)
                .Select(c => CheckInMapper.ToGetDto(c))
                .ToPagedResultAsync(request, cancellationToken);

            return ApiResponse<PagedResult<GetCheckInDto>>.Success(checkIns);
        }
    }
}
