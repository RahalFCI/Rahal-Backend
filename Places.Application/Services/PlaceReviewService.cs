using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Places.Application.DTOs.PlaceReview;
using Places.Application.Interfaces;
using Places.Application.Mappers;
using Places.Domain.Entities;

namespace Places.Application.Services
{
    internal class PlaceReviewService : IPlaceReviewService
    {
        private readonly IGenericRepository<PlaceReview> _reviewRepository;
        private readonly IGenericRepository<Place> _placeRepository;
        private readonly IGenericRepository<CheckIn> _checkInRepository;
        private readonly ILogger<PlaceReviewService> _logger;

        public PlaceReviewService(
            IGenericRepository<PlaceReview> reviewRepository,
            IGenericRepository<Place> placeRepository,
            IGenericRepository<CheckIn> checkInRepository,
            ILogger<PlaceReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _placeRepository = placeRepository;
            _checkInRepository = checkInRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetPlaceReviewDto>> GetPlaceReviewAsync(Guid explorerId, Guid placeId, Guid checkInId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching review for explorer {ExplorerId} on place {PlaceId}", explorerId, placeId);

            var review = await _reviewRepository.GetTable()
                .Include(r => r.Place)
                .FirstOrDefaultAsync(r => r.ExplorerId == explorerId && r.PlaceId == placeId && r.CheckInId == checkInId, cancellationToken);

            if (review is null)
            {
                _logger.LogWarning("Review not found for explorer {ExplorerId} on place {PlaceId}", explorerId, placeId);
                return ApiResponse<GetPlaceReviewDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<GetPlaceReviewDto>.Success(PlaceReviewMapper.ToGetDto(review));
        }

        public async Task<ApiResponse<IEnumerable<GetPlaceReviewDto>>> GetReviewsByPlaceIdAsync(Guid placeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching reviews for place {PlaceId}", placeId);

            var place = await _placeRepository.GetByIdAsync(placeId, cancellationToken);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found", placeId);
                return ApiResponse<IEnumerable<GetPlaceReviewDto>>.Failure(ErrorCode.NotFound);
            }

            var reviews = await _reviewRepository.GetTable()
                .Where(r => r.PlaceId == placeId)
                .Include(r => r.Place)
                .ToListAsync(cancellationToken);

            var dtos = PlaceReviewMapper.ToGetDtos(reviews);
            _logger.LogInformation("Retrieved {ReviewCount} reviews for place {PlaceId}", reviews.Count(), placeId);

            return ApiResponse<IEnumerable<GetPlaceReviewDto>>.Success(dtos);
        }

        public async Task<ApiResponse<IEnumerable<GetPlaceReviewDto>>> GetReviewsByExplorerIdAsync(Guid explorerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching reviews by explorer {ExplorerId}", explorerId);

            var reviews = await _reviewRepository.GetTable()
                .Where(r => r.ExplorerId == explorerId)
                .Include(r => r.Place)
                .ToListAsync(cancellationToken);

            var dtos = PlaceReviewMapper.ToGetDtos(reviews);
            _logger.LogInformation("Retrieved {ReviewCount} reviews by explorer {ExplorerId}", reviews.Count(), explorerId);

            return ApiResponse<IEnumerable<GetPlaceReviewDto>>.Success(dtos);
        }

        public async Task<ApiResponse<string>> CreatePlaceReviewAsync(CreatePlaceReviewDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating review for explorer {ExplorerId} on place {PlaceId}", dto.ExplorerId, dto.PlaceId);

            var place = await _placeRepository.GetByIdAsync(dto.PlaceId, cancellationToken);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found", dto.PlaceId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var checkIn = await _checkInRepository.GetTable()
                .FirstOrDefaultAsync(c => c.ExplorerId == dto.ExplorerId && c.PlaceId == dto.PlaceId && c.Id == dto.CheckInId, cancellationToken);

            if (checkIn is null)
            {
                _logger.LogWarning("Check-in not found for explorer {ExplorerId} at place {PlaceId}", dto.ExplorerId, dto.PlaceId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var existingReview = await _reviewRepository.GetTable()
                .FirstOrDefaultAsync(r => r.ExplorerId == dto.ExplorerId && r.PlaceId == dto.PlaceId && r.CheckInId == dto.CheckInId, cancellationToken);

            if (existingReview is not null)
            {
                _logger.LogWarning("Review already exists for explorer {ExplorerId} on place {PlaceId}", dto.ExplorerId, dto.PlaceId);
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            var review = PlaceReviewMapper.ToEntity(dto);
            _reviewRepository.Add(review);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review created successfully for explorer {ExplorerId} on place {PlaceId}", dto.ExplorerId, dto.PlaceId);

            return ApiResponse<string>.Success("Review created successfully");
        }

        public async Task<ApiResponse<string>> UpdatePlaceReviewAsync(Guid explorerId, Guid placeId, Guid checkInId, UpdatePlaceReviewDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating review for explorer {ExplorerId} on place {PlaceId}", explorerId, placeId);

            var review = await _reviewRepository.GetTable()
                .FirstOrDefaultAsync(r => r.ExplorerId == explorerId && r.PlaceId == placeId && r.CheckInId == checkInId, cancellationToken);

            if (review is null)
            {
                _logger.LogWarning("Review not found for explorer {ExplorerId} on place {PlaceId}", explorerId, placeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            PlaceReviewMapper.UpdateEntity(review, dto);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review updated successfully for explorer {ExplorerId} on place {PlaceId}", explorerId, placeId);

            return ApiResponse<string>.Success("Review updated successfully");
        }

        public async Task<ApiResponse<string>> DeletePlaceReviewAsync(Guid explorerId, Guid placeId, Guid checkInId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting review for explorer {ExplorerId} on place {PlaceId}", explorerId, placeId);

            var review = await _reviewRepository.GetTable()
                .FirstOrDefaultAsync(r => r.ExplorerId == explorerId && r.PlaceId == placeId && r.CheckInId == checkInId, cancellationToken);

            if (review is null)
            {
                _logger.LogWarning("Review not found for explorer {ExplorerId} on place {PlaceId}", explorerId, placeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            review.IsDeleted = true;
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review deleted successfully for explorer {ExplorerId} on place {PlaceId}", explorerId, placeId);

            return ApiResponse<string>.Success("Review deleted successfully");
        }

        public async Task<ApiResponse<IEnumerable<GetPlaceReviewDto>>> GetVerifiedReviewsByPlaceIdAsync(Guid placeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching verified reviews for place {PlaceId}", placeId);

            var place = await _placeRepository.GetByIdAsync(placeId, cancellationToken);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found", placeId);
                return ApiResponse<IEnumerable<GetPlaceReviewDto>>.Failure(ErrorCode.NotFound);
            }

            var reviews = await _reviewRepository.GetTable()
                .Where(r => r.PlaceId == placeId && r.IsVerified)
                .Include(r => r.Place)
                .ToListAsync(cancellationToken);

            var dtos = PlaceReviewMapper.ToGetDtos(reviews);
            _logger.LogInformation("Retrieved {ReviewCount} verified reviews for place {PlaceId}", reviews.Count(), placeId);

            return ApiResponse<IEnumerable<GetPlaceReviewDto>>.Success(dtos);
        }
    }
}
