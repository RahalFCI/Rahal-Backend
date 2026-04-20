using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Places.Domain.Entities;

namespace Places.Infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Entity configuration for PlacePhoto
    /// Configures table mapping, indexes, and constraints
    /// Note: PlacePhoto does not inherit from BaseEntity, so no soft delete filter is applied
    /// </summary>
    public class PlacePhotoConfiguration : IEntityTypeConfiguration<PlacePhoto>
    {
        public void Configure(EntityTypeBuilder<PlacePhoto> builder)
        {
            builder.ToTable("PlacePhotos", "places");

            // Primary Key (composite)
            builder.HasKey(e => new { e.PlaceId, e.Url });

            // Domain Properties
            builder.Property(e => e.PlaceId)
                .IsRequired();

            builder.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500)
                .HasComment("URL of the place photo");

            // Foreign Key to Place
            builder.HasOne(e => e.Place)
                .WithMany()
                .HasForeignKey(e => e.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => e.PlaceId)
                .HasDatabaseName("IX_PlacePhotos_PlaceId");

            builder.HasIndex(e => e.Url)
                .HasDatabaseName("IX_PlacePhotos_Url");
        }
    }
}
