using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Persistence.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Posts", "socialmedia");

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

            // UserId references users.AspNetUsers (cross-module — no EF FK constraint)
            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(e => e.IsPublic)
                .IsRequired()
                .HasDefaultValue(true);

            // List<string> stored as JSONB — PostgreSQL native array/json column
            builder.Property(e => e.MediaUrls)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb")
                .HasComment("Ordered list of media URLs (images/videos) stored as JSONB");

            // Relationships (within SocialMedia schema)
            builder.HasMany(e => e.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Likes)
                .WithOne(l => l.Post)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.PostPlaces)
                .WithOne(pp => pp.Post)
                .HasForeignKey(pp => pp.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes

            // Composite index: profile timeline — fetch all posts by a user ordered by newest first
            builder.HasIndex(e => new { e.UserId, e.CreatedAt })
                .HasDatabaseName("IX_Posts_UserId_CreatedAt")
                .IsDescending(false, true); // UserId ASC, CreatedAt DESC

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Posts_IsDeleted");
        }
    }
}
