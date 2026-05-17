using Meilisearch;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Search.Vendor
{
    public class VendorIndexConfig : ISearchIndexInitializer
    {
        public string IndexName => "vendorsearchdocument";

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
                    new[] { "name" }, cancellationToken);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to configure vendors index: {ex.Message}");
            }
        }
    }
}
