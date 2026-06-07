using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Gamification.Infrastructure.Search.Vendor
{
    public class VendorSearchDocument
    {
        [JsonPropertyName("id")]
        public Guid UserId { get; set; } = Guid.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string ProfilePictureUrl { get; set; } = string.Empty;

    }
}
