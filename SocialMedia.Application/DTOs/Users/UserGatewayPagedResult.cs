namespace SocialMedia.Application.DTOs.Users
{
    public class UserGatewayPagedResult
    {
        public IEnumerable<UserGatewayUserDto> Items { get; set; } = Enumerable.Empty<UserGatewayUserDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
