using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Places.Domain.Entities;

namespace Places.Infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Entity configuration for PlaceCategory
    /// Configures table mapping, indexes, constraints, and soft delete query filter
    /// </summary>
    public class PlaceCategoryConfiguration : IEntityTypeConfiguration<PlaceCategory>
    {
        public void Configure(EntityTypeBuilder<PlaceCategory> builder)
        {
            builder.ToTable("PlaceCategories", "places");

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
                .HasMaxLength(150);

            // Large string mapped to TEXT type in database
            builder.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("text")
                .HasComment("Detailed description of the place category");

            // Relationships
            builder.HasMany(e => e.Places)
                .WithOne(p => p.PlaceCategory)
                .HasForeignKey(p => p.PlaceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_PlaceCategories_Name_Unique");

            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_PlaceCategories_CreatedAt");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_PlaceCategories_IsDeleted");
        }
    }
}
