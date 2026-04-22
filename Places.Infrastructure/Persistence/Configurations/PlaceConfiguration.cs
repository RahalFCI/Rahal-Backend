using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Places.Domain.Entities;

namespace Places.Infrastructure.Persistence.Configuration
{

    public class PlaceConfiguration : IEntityTypeConfiguration<Place>
    {
        public void Configure(EntityTypeBuilder<Place> builder)
        {
            builder.ToTable("Places", "places");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Audit Properties (inherited from BaseEntity)
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Domain Properties
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.AddressLine)
                    .HasMaxLength(200);
                address.Property(a => a.City)
                    .HasMaxLength(200)
                    .IsRequired();
                address.Property(a => a.Government)
                    .HasMaxLength(200)
                    .IsRequired();
            });

            // Large string mapped to TEXT type in database
            builder.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("text")
                .HasComment("Detailed description of the place");

            builder.Property(e => e.TicketPrice)
                .HasDefaultValue(0.0)
                .HasPrecision(10, 2);

            builder.Property(e => e.Latitude)
                .IsRequired()
                .HasPrecision(10, 8)
                .HasComment("Geographic latitude coordinate");

            builder.Property(e => e.Longitude)
                .IsRequired()
                .HasPrecision(11, 8)
                .HasComment("Geographic longitude coordinate");

            builder.Property(e => e.GeofenceRange)
                .IsRequired()
                .HasDefaultValue(100)
                .HasComment("Radius in meters for geofence validation");

            // Foreign Key to PlaceCategory
            builder.HasOne(e => e.PlaceCategory)
                .WithMany(c => c.Places)
                .HasForeignKey(e => e.PlaceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            builder.HasMany<PlacePhoto>()
                .WithOne(p => p.Place)
                .HasForeignKey(p => p.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany<PlaceReview>()
                .WithOne(r => r.Place)
                .HasForeignKey(r => r.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany<CheckIn>()
                .WithOne(c => c.Place)
                .HasForeignKey(c => c.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Places_Name");

            builder.HasIndex(e => e.PlaceCategoryId)
                .HasDatabaseName("IX_Places_PlaceCategoryId");

            builder.HasIndex(e => new { e.Latitude, e.Longitude })
                .HasDatabaseName("IX_Places_Coordinates");

            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Places_CreatedAt");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Places_IsDeleted");

            builder.HasIndex(e => e.Address!.City)
                .HasDatabaseName("IX_Places_City");

            builder.HasIndex(e => e.Address!.Government)
                .HasDatabaseName("IX_Places_Government");
        }
    }
}
