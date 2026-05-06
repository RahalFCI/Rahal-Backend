using Microsoft.AspNetCore.Http;

namespace Gamification.Application.DTOs.Badge
{
    public class CreateBadgeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
