namespace Shared.Application.Search
{
    /// <summary>
    /// Generic search result wrapper containing hits, pagination info, and total count.
    /// </summary>
    /// <typeparam name="T">Type of search result items</typeparam>
    public class SearchResult<T>
    {
        public IEnumerable<T> Hits { get; set; } = Enumerable.Empty<T>();
        public long TotalHits { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalHits / PageSize) : 0;
        public bool HasMore => Page < TotalPages;

        public static SearchResult<T> Empty(int page = 1, int pageSize = 10)
        {
            return new SearchResult<T>
            {
                Hits = Enumerable.Empty<T>(),
                TotalHits = 0,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
