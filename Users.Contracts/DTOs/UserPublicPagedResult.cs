namespace Users.Contracts.DTOs
{
    public class UserPublicPagedResult
    {
        public IEnumerable<UserPublicDto> Items { get; set; } = Enumerable.Empty<UserPublicDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
