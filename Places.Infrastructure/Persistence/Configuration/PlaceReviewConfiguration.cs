using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Places.Domain.Entities;

namespace Places.Infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Entity configuration for PlaceReview
    /// Configures table mapping, indexes, and constraints
    /// Note: PlaceReview does not inherit from BaseEntity, so no soft delete filter is applied
    /// </summary>
    public class PlaceReviewConfiguration : IEntityTypeConfiguration<PlaceReview>
    {
        public void Configure(EntityTypeBuilder<PlaceReview> builder)
        {
            builder.ToTable("PlaceReviews", "places");

            // Primary Key (composite)
            builder.HasKey(e => new { e.ExplorerId, e.PlaceId, e.CheckInId });

            // Domain Properties
            builder.Property(e => e.ExplorerId)
                .IsRequired();

            builder.Property(e => e.PlaceId)
                .IsRequired();

            builder.Property(e => e.CheckInId)
                .IsRequired();

            builder.Property(e => e.Rating)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Rating value (typically 1-5)");

            // Large string mapped to TEXT type in database
            builder.Property(e => e.Comment)
                .IsRequired()
                .HasColumnType("text")
                .HasComment("Review comment/feedback from explorer");

            builder.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasComment("Whether the review is verified by moderators");

            // Foreign Keys
            builder.HasOne(e => e.Place)
                .WithMany()
                .HasForeignKey(e => e.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.CheckIn)
                .WithMany()
                .HasForeignKey(e => e.CheckInId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => e.PlaceId)
                .HasDatabaseName("IX_PlaceReviews_PlaceId");

            builder.HasIndex(e => e.ExplorerId)
                .HasDatabaseName("IX_PlaceReviews_ExplorerId");

            builder.HasIndex(e => e.Rating)
                .HasDatabaseName("IX_PlaceReviews_Rating");

            builder.HasIndex(e => e.IsVerified)
                .HasDatabaseName("IX_PlaceReviews_IsVerified");

            builder.HasIndex(e => new { e.PlaceId, e.Rating })
                .HasDatabaseName("IX_PlaceReviews_PlaceId_Rating");
        }
    }
}
