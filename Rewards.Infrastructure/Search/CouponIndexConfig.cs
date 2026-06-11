using Meilisearch;
using Shared.Application.Interfaces;

namespace Rewards.Infrastructure.Search
{
    public class CouponIndexConfig : ISearchIndexInitializer
    {
        public string IndexName => "couponsearchdocument";

        public async Task ConfigureAsync(object client, CancellationToken cancellationToken = default)
        {
            if (client is not MeilisearchClient meilisearchClient)
                throw new ArgumentException("Client must be a MeilisearchClient instance", nameof(client));

            try
            {
                await meilisearchClient.CreateIndexAsync(IndexName, primaryKey: "id", cancellationToken: cancellationToken);
            }
            catch
            {
            }

            var index = meilisearchClient.Index(IndexName);
            await index.UpdateSearchableAttributesAsync(new[] { "title", "description", "discountType" }, cancellationToken);
            await index.UpdateFilterableAttributesAsync(new[] { "vendorId", "discountType", "xpCost", "minimumCharge", "expiresAt", "isActive" }, cancellationToken);
            await index.UpdateSortableAttributesAsync(new[] { "xpCost", "minimumCharge", "expiresAt" }, cancellationToken);
        }
    }
}
