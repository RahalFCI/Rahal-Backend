namespace Shared.Application.Settings
{
    /// <summary>
    /// Configuration settings for Meilisearch integration.
    /// Read from appsettings under "Search:Meilisearch" section.
    /// </summary>
    public class MeilisearchSettings
    {
        public const string SectionName = "Meilisearch";
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public bool IsValid() => !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(ApiKey);
    }
}
