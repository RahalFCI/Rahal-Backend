namespace SocialMedia.Domain.Entities
{
    /// <summary>
    /// Junction entity tagging a Post with a Place.
    /// Composite primary key (PostId, PlaceId) — does NOT inherit BaseEntity.
    /// PlaceId references places.Places (cross-module — no EF navigation).
    /// </summary>
    public class PostPlace
    {
        public Guid PostId { get; set; }
        public Post? Post { get; set; }

        /// <summary>
        /// References places.Places — stored as plain Guid (cross-module, no EF navigation).
        /// </summary>
        public Guid PlaceId { get; set; }
    }
}
