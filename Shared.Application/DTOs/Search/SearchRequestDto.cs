namespace Shared.Application.DTOs.Search
{
    /// <summary>
    /// Request DTO for search operations
    /// Contains all parameters needed for performing a search query
    /// </summary>
    public class SearchRequestDto
    {

        public string Query { get; set; } = string.Empty;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? Filter { get; set; }

        public string? SortBy { get; set; }
    }
}
