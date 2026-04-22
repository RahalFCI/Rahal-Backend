using Places.Application.DTOs.PlacePhoto;
using Places.Domain.Entities;

namespace Places.Application.Mappers
{
    public static class PlacePhotoMapper
    {
        public static GetPlacePhotoDto ToGetDto(PlacePhoto photo)
        {
            return new GetPlacePhotoDto
            {
                PlaceId = photo.PlaceId,
                Url = photo.Url
            };
        }

        public static PlacePhoto ToEntity(AddPlacePhotoDto dto)
        {
            return new PlacePhoto
            {
                PlaceId = dto.PlaceId,
                Url = dto.Url
            };
        }

        public static IEnumerable<GetPlacePhotoDto> ToGetDtos(IEnumerable<PlacePhoto> photos)
        {
            return photos.Select(ToGetDto);
        }
    }
}
