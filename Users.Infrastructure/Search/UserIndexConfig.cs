using Meilisearch;
using Shared.Application.Interfaces;

namespace Users.Infrastructure.Search
{
    public class UserIndexConfig : ISearchIndexInitializer
    {
        public string IndexName => "usersearchdocument";

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
                    new[] { "fullName", "username", "email" }, cancellationToken);

            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine($"Failed to configure users index: {ex.Message}");
            }
        }
    }
}
