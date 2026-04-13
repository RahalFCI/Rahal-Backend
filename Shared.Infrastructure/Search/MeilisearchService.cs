using Meilisearch;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Shared.Application.Interfaces;
using Shared.Application.Search;

namespace Shared.Infrastructure.Search
{
    public class MeilisearchService<T> : ISearchService<T>
    {
        private readonly MeilisearchClient _client;
        private readonly ResiliencePipeline _resiliencePipeline;
        private readonly ILogger<MeilisearchService<T>> _logger;
        private readonly string _indexName;

        public MeilisearchService(
            MeilisearchClient client,
            ResiliencePipeline resiliencePipeline,
            ILogger<MeilisearchService<T>> logger)
        {
            _client = client;
            _resiliencePipeline = resiliencePipeline;
            _logger = logger;
            // Index name derived from type name by convention (lowercase)
            _indexName = typeof(T).Name.ToLowerInvariant();
        }

        public async Task IndexAsync(T document, CancellationToken cancellationToken = default)
        {
            if (document == null)
            {
                _logger.LogWarning("Attempted to index null document of type {DocumentType}", typeof(T).Name);
                return;
            }

            try
            {
                await _resiliencePipeline.ExecuteAsync(
                    async ct => await IndexInternalAsync(document, ct),
                    cancellationToken);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "Circuit breaker open: Cannot index document of type {DocumentType} - Meilisearch temporarily unavailable",
                    typeof(T).Name);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex,
                    "Operation canceled: Cannot index document of type {DocumentType}",
                    typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to index document of type {DocumentType} after resilience policy exhaustion",
                    typeof(T).Name);
            }
        }

        public async Task IndexBatchAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default)
        {
            var documentList = documents?.ToList();
            if (documentList == null || !documentList.Any())
            {
                _logger.LogWarning("Attempted to index empty batch of {DocumentType}", typeof(T).Name);
                return;
            }

            try
            {
                await _resiliencePipeline.ExecuteAsync(
                    async ct => await IndexBatchInternalAsync(documentList, ct),
                    cancellationToken);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "Circuit breaker open: Cannot index batch of {DocumentType} - Meilisearch temporarily unavailable",
                    typeof(T).Name);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex,
                    "Operation canceled: Cannot index batch of {DocumentType}",
                    typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to index batch of {DocumentCount} {DocumentType} documents after resilience policy exhaustion",
                    documentList.Count,
                    typeof(T).Name);
            }
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Attempted to delete document with null or empty ID from {IndexName}", _indexName);
                return;
            }

            try
            {
                await _resiliencePipeline.ExecuteAsync(
                    async ct => await DeleteInternalAsync(id, ct),
                    cancellationToken);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "Circuit breaker open: Cannot delete document '{DocumentId}' - Meilisearch temporarily unavailable",
                    id);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex,
                    "Operation canceled: Cannot delete document '{DocumentId}'",
                    id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to delete document '{DocumentId}' from '{IndexName}' after resilience policy exhaustion",
                    id,
                    _indexName);
            }
        }

        public async Task<Shared.Application.Search.SearchResult<T>> SearchAsync(
            string query,
            SearchOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogDebug("Search query is empty or whitespace");
                return Shared.Application.Search.SearchResult<T>.Empty(options?.Page ?? 1, options?.PageSize ?? 10);
            }

            var searchOptions = options ?? new SearchOptions();
            if (!searchOptions.IsValid())
            {
                _logger.LogWarning("Invalid search options: Page={Page}, PageSize={PageSize}", searchOptions.Page, searchOptions.PageSize);
                return Shared.Application.Search.SearchResult<T>.Empty();
            }

            try
            {
                return await _resiliencePipeline.ExecuteAsync(
                    async ct => await SearchInternalAsync(query, searchOptions, ct),
                    cancellationToken);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "Circuit breaker open: Cannot search in '{IndexName}' - Meilisearch temporarily unavailable",
                    _indexName);
                return Shared.Application.Search.SearchResult<T>.Empty(searchOptions.Page, searchOptions.PageSize);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex,
                    "Operation canceled: Cannot search in '{IndexName}'",
                    _indexName);
                return Shared.Application.Search.SearchResult<T>.Empty(searchOptions.Page, searchOptions.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Search for '{Query}' in '{IndexName}' failed after resilience policy exhaustion",
                    query,
                    _indexName);
                return Shared.Application.Search.SearchResult<T>.Empty(searchOptions.Page, searchOptions.PageSize);
            }
        }

        private async ValueTask IndexInternalAsync(T document, CancellationToken cancellationToken)
        {
            var index = _client.Index(_indexName);
            await index.AddDocumentsAsync(new[] { document }, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Document of type {DocumentType} successfully indexed in '{IndexName}'",
                typeof(T).Name,
                _indexName);
        }

        private async ValueTask IndexBatchInternalAsync(IList<T> documents, CancellationToken cancellationToken)
        {
            var index = _client.Index(_indexName);
            await index.AddDocumentsAsync(documents, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Batch of {DocumentCount} documents of type {DocumentType} successfully indexed in '{IndexName}'",
                documents.Count,
                typeof(T).Name,
                _indexName);
        }

        private async ValueTask DeleteInternalAsync(string id, CancellationToken cancellationToken)
        {
            var index = _client.Index(_indexName);
            // Use DeleteDocumentsAsync with a list containing the single ID
            await index.DeleteDocumentsAsync(new[] { id }, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Document with ID '{DocumentId}' successfully deleted from '{IndexName}'",
                id,
                _indexName);
        }

        private async ValueTask<Shared.Application.Search.SearchResult<T>> SearchInternalAsync(
            string query,
            SearchOptions searchOptions,
            CancellationToken cancellationToken)
        {
            var index = _client.Index(_indexName);

            // Perform the search with query string
            var result = await index.SearchAsync<T>(query, cancellationToken: cancellationToken);

            // Get total hits
            long totalHits = 0;
            if (result is ISearchable<T> searchable)
            {
                // Try to get count from hits if available
                totalHits = result.Hits?.LongCount() ?? 0;
            }

            _logger.LogInformation(
                "Search query '{Query}' in '{IndexName}' returned {HitCount} results",
                query,
                _indexName,
                result.Hits?.Count() ?? 0);

            return new Shared.Application.Search.SearchResult<T>
            {
                Hits = result.Hits ?? Enumerable.Empty<T>(),
                TotalHits = totalHits,
                Page = searchOptions.Page,
                PageSize = searchOptions.PageSize
            };
        }
    }
}
