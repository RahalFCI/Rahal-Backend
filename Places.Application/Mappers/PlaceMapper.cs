using Places.Application.DTOs.Place;
using Places.Domain.Entities;

namespace Places.Application.Mappers
{
    public static class PlaceMapper
    {
        public static GetPlaceDto ToGetDto(Place place)
        {
            return new GetPlaceDto
            {
                Id = place.Id,
                Name = place.Name,
                Description = place.Description,
                PlaceCategoryId = place.PlaceCategoryId,
                CategoryName = place.PlaceCategory?.Name ?? string.Empty,
                TicketPrice = place.TicketPrice,
                Latitude = place.Latitude,
                Longitude = place.Longitude,
                GeoFenceRange = place.GeofenceRange,
                CreatedAt = place.CreatedAt,
                UpdatedAt = place.UpdatedAt
            };
        }

        public static Place ToEntity(CreatePlaceDto dto)
        {
            return new Place
            {
                Name = dto.Name,
                Description = dto.Description,
                PlaceCategoryId = dto.PlaceCategoryId,
                TicketPrice = dto.TicketPrice,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                GeofenceRange = dto.GeoFenceRange
            };
        }

        public static void UpdateEntity(Place place, UpdatePlaceDto dto)
        {
            place.Name = dto.Name;
            place.Description = dto.Description;
            place.PlaceCategoryId = dto.PlaceCategoryId;
            place.TicketPrice = dto.TicketPrice;
            place.Latitude = dto.Latitude;
            place.Longitude = dto.Longitude;
            place.GeofenceRange = dto.GeoFenceRange;
        }

        public static IEnumerable<GetPlaceDto> ToGetDtos(IEnumerable<Place> places)
        {
            return places.Select(ToGetDto);
        }
    }
}
