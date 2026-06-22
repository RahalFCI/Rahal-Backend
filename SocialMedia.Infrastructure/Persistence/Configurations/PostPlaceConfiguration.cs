using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Persistence.Configurations
{
    public class PostPlaceConfiguration : IEntityTypeConfiguration<PostPlace>
    {
        public void Configure(EntityTypeBuilder<PostPlace> builder)
        {
            builder.ToTable("PostPlaces", "socialmedia");

            // Composite Primary Key
            builder.HasKey(e => new { e.PostId, e.PlaceId });

            builder.Property(e => e.PostId)
                .IsRequired();

            // PlaceId references places.Places (cross-module — no EF FK constraint)
            builder.Property(e => e.PlaceId)
                .IsRequired();

            // FK to Post (within SocialMedia schema)
            // Navigation marked as optional because Post has a global soft-delete query filter;
            // EF warns if the required end of a relationship is filtered out.
            builder.HasOne(e => e.Post)
                .WithMany(p => p.PostPlaces)
                .HasForeignKey(e => e.PostId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes

            // B-Tree index on PlaceId — fetch all posts tagged at a specific place
            builder.HasIndex(e => e.PlaceId)
                .HasDatabaseName("IX_PostPlaces_PlaceId");
        }
    }
}
