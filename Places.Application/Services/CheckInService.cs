using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Application.DTOs.CheckIn;
using Places.Application.Factories;
using Places.Application.Interfaces;
using Places.Application.Mappers;
using Places.Domain.Entities;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Places.Application.Services
{
    internal class CheckInService : ICheckInService
    {
        private readonly IGenericRepository<CheckIn> _checkInRepository;
        private readonly IGenericRepository<Place> _placeRepository;
        private readonly ICheckInValidatorService _validator;
        private readonly ILogger<CheckInService> _logger;

        public CheckInService(
            IGenericRepository<CheckIn> checkInRepository,
            IGenericRepository<Place> placeRepository,
            ICheckInValidatorService validator,
            ILogger<CheckInService> logger)
        {
            _checkInRepository = checkInRepository;
            _placeRepository = placeRepository;
            _validator = validator;
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

        public async Task<ApiResponse<IEnumerable<GetCheckInDto>>> GetAllCheckInAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all check-ins");

            var checkIns = await _checkInRepository.GetTable()
                .AsNoTracking()
                .Include(c => c.Place)
                .Select(c => CheckInMapper.ToGetDto(c))
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Fetched {Count} check-ins", checkIns.Count);

            return ApiResponse<IEnumerable<GetCheckInDto>>.Success(checkIns);
        }

        public async Task<ApiResponse<IEnumerable<GetCheckInDto>>> GetCheckInsByPlaceIdAsync(Guid placeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching check-ins for place {PlaceId}", placeId);

            var place = await _placeRepository.GetByIdAsync(placeId, cancellationToken);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found", placeId);
                return ApiResponse<IEnumerable<GetCheckInDto>>.Failure(ErrorCode.NotFound);
            }

            var checkIns = await _checkInRepository.GetTable()
                .AsNoTracking()
                .Where(c => c.PlaceId == placeId)
                .Include(c => c.Place)
                .ToListAsync(cancellationToken);

            var dtos = CheckInMapper.ToGetDtos(checkIns);
            _logger.LogInformation("Retrieved {CheckInCount} check-ins for place {PlaceId}", checkIns.Count(), placeId);

            return ApiResponse<IEnumerable<GetCheckInDto>>.Success(dtos);
        }

        public async Task<ApiResponse<IEnumerable<GetCheckInDto>>> GetCheckInsByExplorerIdAsync(Guid explorerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching check-ins for explorer {ExplorerId}", explorerId);

            var checkIns = await _checkInRepository.GetTable()
                .AsNoTracking()
                .Where(c => c.ExplorerId == explorerId)
                .Include(c => c.Place)
                .ToListAsync(cancellationToken);

            var dtos = CheckInMapper.ToGetDtos(checkIns);
            _logger.LogInformation("Retrieved {CheckInCount} check-ins for explorer {ExplorerId}", checkIns.Count(), explorerId);

            return ApiResponse<IEnumerable<GetCheckInDto>>.Success(dtos);
        }

        public async Task<ApiResponse<string>> CheckInAsync(
            Guid explorerId,
            CheckInRequestDto request,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Processing check-in request for explorer {ExplorerId} at place {PlaceId}", 
                explorerId, request.PlaceId);

            var place = await _placeRepository.GetByIdAsync(request.PlaceId, ct);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found for check-in", request.PlaceId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
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

        public async Task<ApiResponse<IEnumerable<GetCheckInDto>>> GetPendingCheckInsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching pending check-ins");

            var checkIns = await _checkInRepository.GetTable()
                .AsNoTracking()
                .Where(c => c.ValidationStatus == Places.Domain.Enums.CheckInValidationStatus.Pending)
                .Include(c => c.Place)
                .ToListAsync(cancellationToken);

            var dtos = CheckInMapper.ToGetDtos(checkIns);
            _logger.LogInformation("Retrieved {CheckInCount} pending check-ins", checkIns.Count());

            return ApiResponse<IEnumerable<GetCheckInDto>>.Success(dtos);
        }
    }
}

