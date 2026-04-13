namespace Shared.Application.Search
{
    /// <summary>
    /// Strongly-typed search options for filtering, sorting, and pagination.
    /// </summary>
    public class SearchOptions
    {
        public string Query { get; set; } = string.Empty;
        public string? Filter { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IsValid()
        {
            return Page > 0 && PageSize > 0 && PageSize <= 1000;
        }
    }
}
