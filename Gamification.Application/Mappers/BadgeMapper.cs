using Gamification.Application.DTOs.Badge;
using Gamification.Domain.Entities;

namespace Gamification.Application.Mappers
{
    public static class BadgeMapper
    {
        public static GetBadgeDto ToGetDto(Badge badge)
        {
            return new GetBadgeDto
            {
                Id = badge.Id,
                Name = badge.Name,
                Description = badge.Description,
                ImageUrl = badge.ImageUrl,
                CreatedAt = badge.CreatedAt,
                UpdatedAt = badge.UpdatedAt
            };
        }

        public static Badge ToEntity(CreateBadgeDto dto)
        {
            return new Badge
            {
                Name = dto.Name,
                Description = dto.Description,
            };
        }

        public static void UpdateEntity(Badge badge, UpdateBadgeDto dto)
        {
            badge.Name = dto.Name;
            badge.Description = dto.Description;
        }

        public static IEnumerable<GetBadgeDto> ToGetDtos(IEnumerable<Badge?> badges)
        {
            return badges.Where(b => b != null).Select(b => ToGetDto(b!));
        }
    }
}
