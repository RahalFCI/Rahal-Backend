using Meilisearch;
using Shared.Application.Interfaces;

namespace Places.Infrastructure.Search
{
    public class PlaceIndexConfig : ISearchIndexInitializer
    {
        public string IndexName => "placesearchdocument";

        public async Task ConfigureAsync(object client, CancellationToken cancellationToken = default)
        {
            try
            {
                if (client is not MeilisearchClient meilisearchClient)
                {
                    throw new ArgumentException("Client must be a MeilisearchClient instance", nameof(client));
                }

                await meilisearchClient.CreateIndexAsync(IndexName, primaryKey: "id", cancellationToken: cancellationToken);

                var index = meilisearchClient.Index(IndexName);

                await index.UpdateSearchableAttributesAsync(
                    new[] { "name", "description", "categoryName", "city", "country" }, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to configure places index: {ex.Message}");
            }
        }
    }
}
