using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Application.DTOs.PlacePhoto;
using Places.Application.Interfaces;
using Places.Application.Mappers;
using Places.Domain.Entities;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Places.Application.Services
{
    internal class PlacePhotoService : IPlacePhotoService
    {
        private readonly IGenericRepository<PlacePhoto> _photoRepository;
        private readonly IGenericRepository<Place> _placeRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<PlacePhotoService> _logger;

        public PlacePhotoService(
            IGenericRepository<PlacePhoto> photoRepository,
            IGenericRepository<Place> placeRepository,
            IFileStorageService fileStorageService,
            ILogger<PlacePhotoService> logger)
        {
            _photoRepository = photoRepository;
            _placeRepository = placeRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<GetPlacePhotoDto>> GetPlacePhotoAsync(Guid placeId, string url, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching photo for place {PlaceId} with URL {Url}", placeId, url);

            var photo = await _photoRepository.GetTable()
                .FirstOrDefaultAsync(p => p.PlaceId == placeId && p.Url == url, cancellationToken);

            if (photo is null)
            {
                _logger.LogWarning("Photo not found for place {PlaceId} with URL {Url}", placeId, url);
                return ApiResponse<GetPlacePhotoDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<GetPlacePhotoDto>.Success(PlacePhotoMapper.ToGetDto(photo));
        }

        public async Task<ApiResponse<IEnumerable<GetPlacePhotoDto>>> GetPhotosByPlaceIdAsync(Guid placeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching photos for place {PlaceId}", placeId);

            var place = await _placeRepository.GetByIdAsync(placeId, cancellationToken);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found", placeId);
                return ApiResponse<IEnumerable<GetPlacePhotoDto>>.Failure(ErrorCode.NotFound);
            }

            var photos = await _photoRepository.GetTable()
                .Where(p => p.PlaceId == placeId)
                .ToListAsync(cancellationToken);

            var dtos = PlacePhotoMapper.ToGetDtos(photos);
            _logger.LogInformation("Retrieved {PhotoCount} photos for place {PlaceId}", photos.Count(), placeId);

            return ApiResponse<IEnumerable<GetPlacePhotoDto>>.Success(dtos);
        }

        public async Task<ApiResponse<string>> AddPlacePhotoAsync(Guid placeId, IFormFile photo, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding photo to place {PlaceId}", placeId);

            var place = await _placeRepository.GetByIdAsync(placeId, cancellationToken);
            if (place is null)
            {
                _logger.LogWarning("Place {PlaceId} not found", placeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (photo == null || photo.Length == 0)
            {
                _logger.LogInformation("No photo provided for upload");
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var photoUrl = await _fileStorageService.UploadAsync(photo);
            _logger.LogInformation("Photo successfully uploaded to {Url}", photoUrl);

            var photoInstance = new PlacePhoto
            {
                PlaceId = placeId,
                Url = photoUrl,
            };
            _photoRepository.Add(photoInstance);
            await _photoRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Photo added successfully to place {PlaceId}", placeId);

            return ApiResponse<string>.Success("Photo added successfully");
        }

        public async Task<ApiResponse<string>> DeletePlacePhotoAsync(Guid placeId, string url, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting photo from place {PlaceId} with URL {Url}", placeId, url);

            var photo = await _photoRepository.GetTable()
                .FirstOrDefaultAsync(p => p.PlaceId == placeId && p.Url == url, cancellationToken);

            if (photo is null)
            {
                _logger.LogWarning("Photo not found for place {PlaceId} with URL {Url}", placeId, url);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var deletion = _fileStorageService.DeleteAsync(url);

            if (deletion is null)
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);

            _photoRepository.Delete(photo);
            await _photoRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Photo deleted successfully from place {PlaceId}", placeId);

            return ApiResponse<string>.Success("Photo deleted successfully");
        }

        public async Task<ApiResponse<IEnumerable<GetPlacePhotoDto>>> GetPhotosByPlaceIdsAsync(IEnumerable<Guid> placeIds, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching photos for {PlaceCount} places", placeIds.Count());

            var photos = await _photoRepository.GetTable()
                .Where(p => placeIds.Contains(p.PlaceId))
                .ToListAsync(cancellationToken);

            var dtos = PlacePhotoMapper.ToGetDtos(photos);
            _logger.LogInformation("Retrieved {PhotoCount} photos for {PlaceCount} places", photos.Count(), placeIds.Count());

            return ApiResponse<IEnumerable<GetPlacePhotoDto>>.Success(dtos);
        }
    }
}
