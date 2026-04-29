namespace Places.Application.Helpers
{

    public static class GeoLocationHelper
    {
        private const double EarthRadiusInMeters = 6371000;

        public static double CalculateDistanceInMeters(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            // Convert latitude and longitude differences from degrees to radians
            var latitudeDifference = (latitude2 - latitude1) * Math.PI / 180;
            var longitudeDifference = (longitude2 - longitude1) * Math.PI / 180;

            // Haversine formula:
            // a = sin²(Δlat/2) + cos(lat1) * cos(lat2) * sin²(Δlon/2)
            var a = Math.Sin(latitudeDifference / 2) * Math.Sin(latitudeDifference / 2) +
                    Math.Cos(latitude1 * Math.PI / 180) * Math.Cos(latitude2 * Math.PI / 180) *
                    Math.Sin(longitudeDifference / 2) * Math.Sin(longitudeDifference / 2);

            // c = 2 * atan2(√a, √(1-a)) — angular distance in radians
            var angularDistance = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Multiply angular distance by Earth's radius to get meters
            return EarthRadiusInMeters * angularDistance;
        }

        public static double CalculateDistanceInKilometers(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            return CalculateDistanceInMeters(latitude1, longitude1, latitude2, longitude2) / 1000.0;
        }

        public static bool IsValidCoordinate(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
        }

        public static bool IsPointWithinRadius(
            double centerLatitude,
            double centerLongitude,
            double pointLatitude,
            double pointLongitude,
            int radiusInMeters)
        {
            var distance = CalculateDistanceInMeters(
                centerLatitude,
                centerLongitude,
                pointLatitude,
                pointLongitude);

            return distance <= radiusInMeters;
        }

        public static IEnumerable<T> FilterByRadius<T>(
            IEnumerable<T> locations,
            double centerLatitude,
            double centerLongitude,
            int radiusInMeters,
            Func<T, double> getLatitude,
            Func<T, double> getLongitude)
        {
            return locations.Where(location =>
                IsPointWithinRadius(
                    centerLatitude,
                    centerLongitude,
                    getLatitude(location),
                    getLongitude(location),
                    radiusInMeters));
        }
    }
}
