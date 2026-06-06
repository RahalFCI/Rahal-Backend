using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Application.DTOs.CheckIn;
using Places.Application.Helpers;
using Places.Application.Interfaces;
using Places.Domain.Entities;
using Places.Domain.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Places.Infrastructure.Services
{
    public class GeoCheckInValidatorService : ICheckInValidatorService
    {
        private const double MaxImpossibleSpeedKmH = 900;
        private const double SuspiciousSpeedKmH = 200;
        private const double MaxAccuracyMeters = 50;
        private const int MaxTimestampAgeSeconds = 120;

        private readonly IPlacesRepository<CheckIn> _checkInRepository;
        private readonly ILogger<GeoCheckInValidatorService> _logger;

        public GeoCheckInValidatorService(
            IPlacesRepository<CheckIn> checkInRepository,
            ILogger<GeoCheckInValidatorService> logger)
        {
            _checkInRepository = checkInRepository;
            _logger = logger;
        }

        public async Task<CheckInValidationResult> ValidateAsync(
            CheckInRequestDto request,
            Guid explorerId,
            Place place,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Starting check-in validation for explorer {ExplorerId} at place {PlaceId}", 
                explorerId, place.Id);

            var alreadyCheckedIn = await _checkInRepository
                .GetTable()
                .AsNoTracking()
                .AnyAsync(c => c.ExplorerId == explorerId
                           && c.PlaceId == place.Id
                           && c.ValidationStatus == CheckInValidationStatus.Verified);

            if (alreadyCheckedIn)
            {
                _logger.LogWarning("Explorer {ExplorerId} already checked in to place {PlaceId}", explorerId, place.Id);
                return CheckInValidationResult.HardFailure(
                    ErrorCode.AlreadyCheckedIn,
                    "Already checked in to this place");
            }

            var distanceMeters = GeoLocationHelper.CalculateDistanceInMeters(
                request.Latitude, request.Longitude,
                place.Latitude, place.Longitude);

            if (distanceMeters > place.GeofenceRange)
            {
                _logger.LogWarning("Explorer {ExplorerId} is {Distance}m from place {PlaceId}, geofence range is {Range}m", 
                    explorerId, distanceMeters, place.Id, place.GeofenceRange);
                return CheckInValidationResult.HardFailure(
                    ErrorCode.UserNotAtLocation,
                    $"User is {distanceMeters:F0}m away, place radius is {place.GeofenceRange}m");
            }

            var lastVerifiedCheckIn = await _checkInRepository
                .GetTable()
                .AsNoTracking()
                .Where(c => c.ExplorerId == explorerId
                         && c.ValidationStatus == CheckInValidationStatus.Verified)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (lastVerifiedCheckIn is not null)
            {
                var distance = GeoLocationHelper.CalculateDistanceInMeters(
                    lastVerifiedCheckIn.Latitude, lastVerifiedCheckIn.Longitude,
                    request.Latitude, request.Longitude);

                var timeHours = (DateTime.UtcNow - lastVerifiedCheckIn.CreatedAt).TotalHours;

                if (timeHours > 0)
                {
                    var speedKmH = (distance / 1000) / timeHours;
                    if (speedKmH > MaxImpossibleSpeedKmH)
                    {
                        _logger.LogWarning("Impossible travel speed detected: {Speed}km/h for explorer {ExplorerId}", 
                            speedKmH, explorerId);
                        return CheckInValidationResult.HardFailure(
                            ErrorCode.ImpossibleTravel,
                            $"Travel speed of {speedKmH:F0} km/h is physically impossible");
                    }
                }
            }

            var score = 0;
            var signals = new List<string>();

            if (request.IsMockLocation)
            {
                score += 40;
                signals.Add("Mock location detected");
            }

            if (request.IsJailbroken)
            {
                score += 35;
                signals.Add("Device is jailbroken or rooted");
            }

            if (request.AccuracyMeters > MaxAccuracyMeters)
            {
                score += 20;
                signals.Add($"Poor GPS accuracy: {request.AccuracyMeters}m");
            }

            var ageSeconds = (DateTime.UtcNow - request.CapturedAt).TotalSeconds;
            if (ageSeconds > MaxTimestampAgeSeconds)
            {
                score += 25;
                signals.Add($"Location timestamp is {ageSeconds:F0}s old");
            }

            if (lastVerifiedCheckIn is not null)
            {
                var distance = GeoLocationHelper.CalculateDistanceInMeters(
                    lastVerifiedCheckIn.Latitude, lastVerifiedCheckIn.Longitude,
                    request.Latitude, request.Longitude);

                var timeHours = (DateTime.UtcNow - lastVerifiedCheckIn.CreatedAt).TotalHours;

                if (timeHours > 0)
                {
                    var speedKmH = (distance / 1000) / timeHours;
                    if (speedKmH > SuspiciousSpeedKmH && speedKmH <= MaxImpossibleSpeedKmH)
                    {
                        score += 30;
                        signals.Add($"Suspicious travel speed: {speedKmH:F0} km/h");
                    }
                }
            }

            _logger.LogInformation("Check-in validation completed for explorer {ExplorerId}: score={Score}, isValid={IsValid}", 
                explorerId, score, score <= 60);

            return CheckInValidationResult.FromScore(score, signals);
        }
    }
}
