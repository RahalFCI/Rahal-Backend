using System.Text.Json.Serialization;

namespace Users.Infrastructure.Search
{
    /// <summary>
    /// Search document representing a User in Meilisearch.
    /// Contains only fields relevant for search and display.
    /// </summary>
    public class UserSearchDocument
    {

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("isVerified")]
        public bool IsVerified { get; set; }

        [JsonPropertyName("profilePictureUrl")]
        public string ProfilePictureUrl { get; set; } = string.Empty;

    }
}
