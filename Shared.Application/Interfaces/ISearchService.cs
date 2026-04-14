using Shared.Application.Search;

namespace Shared.Application.Interfaces
{
    public interface ISearchService<T>
    {
        Task IndexAsync(T document, CancellationToken cancellationToken = default);
        Task IndexBatchAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<SearchResult<T>> SearchAsync(string query, SearchOptions? options = null, CancellationToken cancellationToken = default);
    }
}
