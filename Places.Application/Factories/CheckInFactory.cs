using Places.Domain.Entities;
using Places.Domain.Enums;

namespace Places.Application.Factories
{
    public static class CheckInFactory
    {
        public static CheckIn CreateVerified(
            Guid explorerId,
            Guid placeId,
            double latitude,
            double longitude,
            int riskScore)
        {
            var checkIn = new CheckIn
            {
                ExplorerId = explorerId,
                PlaceId = placeId,
                Latitude = latitude,
                Longitude = longitude,
                ValidationStatus = CheckInValidationStatus.Verified
            };
            checkIn.RiskScore = riskScore;
            return checkIn;
        }

        public static CheckIn CreateFailed(
            Guid explorerId,
            Guid placeId,
            double latitude,
            double longitude,
            int riskScore)
        {
            var checkIn = new CheckIn
            {
                ExplorerId = explorerId,
                PlaceId = placeId,
                Latitude = latitude,
                Longitude = longitude,
                ValidationStatus = CheckInValidationStatus.Failed
            };
            checkIn.RiskScore = riskScore;
            return checkIn;
        }
    }
}
