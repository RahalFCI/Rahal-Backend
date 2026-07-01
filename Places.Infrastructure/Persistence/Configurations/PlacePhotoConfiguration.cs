using Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Places.Domain.Entities;

namespace Places.Infrastructure.Persistence.Configuration
{
    public class PlacePhotoConfiguration : BaseAuditableEntityConfiguration<PlacePhoto>
    {
        public override void Configure(EntityTypeBuilder<PlacePhoto> builder)
        {
            base.Configure(builder);

            builder.ToTable("PlacePhotos", "places");

            // Domain Properties
            builder.Property(e => e.PlaceId)
                .IsRequired();

            builder.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500)
                .HasComment("URL of the place photo");

            // Audit Properties (inherited from BaseEntity)

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

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
