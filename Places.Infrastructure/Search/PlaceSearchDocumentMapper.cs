using Places.Domain.Entities;

namespace Places.Infrastructure.Search
{
    public static class PlaceSearchDocumentMapper
    {
        public static PlaceSearchDocument ToSearchDocument(this Place place)
        {
            if (place == null)
                throw new ArgumentNullException(nameof(place));

            return new PlaceSearchDocument
            {
                Id = place.Id.ToString(),
                Name = place.Name ?? string.Empty,
                Description = place.Description ?? string.Empty,
                CategoryName = place.PlaceCategory?.Name ?? string.Empty,
                Latitude = place.Latitude,
                Longitude = place.Longitude,
                TicketPrice = place.TicketPrice,
                City = place.Address?.City ?? string.Empty,
                Country = place.Address?.Country ?? string.Empty,
                Government = place.Address?.Government ?? string.Empty
            };
        }

        public static IEnumerable<PlaceSearchDocument> ToSearchDocuments(this IEnumerable<Place> places)
        {
            return places?.Select(p => p.ToSearchDocument()) ?? Enumerable.Empty<PlaceSearchDocument>();
        }
    }
}
