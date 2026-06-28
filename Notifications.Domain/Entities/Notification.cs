using System.Text.Json;
using Shared.Domain.Entities;

namespace Notifications.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }

        public Guid? ActorId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string? TargetId { get; set; }

        public bool IsRead { get; set; }

        public Dictionary<string, JsonElement> Metadata { get; set; } = new();
    }
}
