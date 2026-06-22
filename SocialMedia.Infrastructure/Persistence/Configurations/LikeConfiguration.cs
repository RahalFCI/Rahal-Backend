using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Persistence.Configurations
{
    public class LikeConfiguration : IEntityTypeConfiguration<Like>
    {
        public void Configure(EntityTypeBuilder<Like> builder)
        {
            builder.ToTable("Likes", "socialmedia");

            // Composite Primary Key — enforces uniqueness: a user can only like a post once
            builder.HasKey(e => new { e.UserId, e.PostId });

            // UserId references users.AspNetUsers (cross-module — no EF FK constraint)
            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.PostId)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            // Foreign Key to Post (within SocialMedia schema)
            // Navigation marked as optional because Post has a global soft-delete query filter;
            // EF warns if the required end of a relationship is filtered out.
            builder.HasOne(e => e.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(e => e.PostId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes

            // B-Tree index on PostId — fetch all users who liked a specific post
            builder.HasIndex(e => e.PostId)
                .HasDatabaseName("IX_Likes_PostId");
        }
    }
}
