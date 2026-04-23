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
                Address = place.Address is not null ? MapAddressToDto(place.Address) : null,
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
                GeofenceRange = dto.GeoFenceRange,
                Address = dto.Address is not null ? MapDtoToAddress(dto.Address) : null
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
            place.Address = dto.Address is not null ? MapDtoToAddress(dto.Address) : null;
        }

        public static IEnumerable<GetPlaceDto> ToGetDtos(IEnumerable<Place> places)
        {
            return places.Select(ToGetDto);
        }

        private static AddressDto MapAddressToDto(Address address)
        {
            return new AddressDto
            {
                AddressLine = address.AddressLine,
                Government = address.Government,
                City = address.City,
                Country = address.Country
            };
        }

        private static Address MapDtoToAddress(AddressDto addressDto)
        {
            return new Address
            {
                AddressLine = addressDto.AddressLine,
                Government = addressDto.Government,
                City = addressDto.City,
                Country = addressDto.Country
            };
        }
    }
}
