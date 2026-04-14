using Meilisearch;
using Shared.Application.Interfaces;

namespace Rahal.Api.Extensions
{
    public static class SearchIndexInitializerExtensions
    {
        public static async Task InitializeSearchIndexesAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<MeilisearchClient>();
            var initializers = scope.ServiceProvider.GetServices<ISearchIndexInitializer>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MeilisearchClient>>();

            foreach (var initializer in initializers)
            {
                try
                {
                    logger.LogInformation("Configuring search index: {IndexName}", initializer.IndexName);
                    await initializer.ConfigureAsync(client, CancellationToken.None);
                    logger.LogInformation("Search index configured successfully: {IndexName}", initializer.IndexName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to configure search index: {IndexName}", initializer.IndexName);
                }
            }
        }
    }
}
