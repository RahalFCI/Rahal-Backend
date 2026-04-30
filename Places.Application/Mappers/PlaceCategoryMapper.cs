using Places.Application.DTOs.PlaceCategory;
using Places.Domain.Entities;

namespace Places.Application.Mappers
{
    public static class PlaceCategoryMapper
    {
        public static GetPlaceCategoryDto ToGetDto(PlaceCategory category)
        {
            return new GetPlaceCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                PlaceCount = category.Places?.Count() ?? 0,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public static PlaceCategory ToEntity(CreatePlaceCategoryDto dto)
        {
            return new PlaceCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                Places = new List<Place>()
            };
        }

        public static void UpdateEntity(PlaceCategory category, UpdatePlaceCategoryDto dto)
        {
            category.Name = dto.Name;
            category.Description = dto.Description;
        }

        public static IEnumerable<GetPlaceCategoryDto> ToGetDtos(IEnumerable<PlaceCategory> categories)
        {
            return categories.Select(ToGetDto);
        }
    }
}
