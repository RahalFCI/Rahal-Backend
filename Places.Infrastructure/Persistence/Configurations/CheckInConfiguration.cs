using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Places.Domain.Entities;

namespace Places.Infrastructure.Persistence.Configuration
{

    public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
    {
        public void Configure(EntityTypeBuilder<CheckIn> builder)
        {
            builder.ToTable("CheckIns", "places");

            // Primary Key (composite)
            builder.HasKey(e => new { e.ExplorerId, e.PlaceId });

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Domain Properties
            builder.Property(e => e.ExplorerId)
                .IsRequired();

            builder.Property(e => e.PlaceId)
                .IsRequired();

            builder.Property(e => e.ValidationStatus)
                .HasDefaultValue(0)
                .HasComment("Validation status of the check-in (Pending=0, Approved=1, Rejected=2)");

            // Foreign Key to Place
            builder.HasOne(e => e.Place)
                .WithMany()
                .HasForeignKey(e => e.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => e.PlaceId)
                .HasDatabaseName("IX_CheckIns_PlaceId");

            builder.HasIndex(e => e.ExplorerId)
                .HasDatabaseName("IX_CheckIns_ExplorerId");

            builder.HasIndex(e => e.ValidationStatus)
                .HasDatabaseName("IX_CheckIns_ValidationStatus");

            builder.HasIndex(e => new { e.PlaceId, e.ValidationStatus })
                .HasDatabaseName("IX_CheckIns_PlaceId_ValidationStatus");
        }
    }
}
