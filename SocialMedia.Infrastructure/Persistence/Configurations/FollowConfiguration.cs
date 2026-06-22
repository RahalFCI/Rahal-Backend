using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Persistence.Configurations
{
    public class FollowConfiguration : IEntityTypeConfiguration<Follow>
    {
        public void Configure(EntityTypeBuilder<Follow> builder)
        {
            builder.ToTable("Follows", "socialmedia");

            // Composite Primary Key
            builder.HasKey(e => new { e.FollowerId, e.FolloweeId });

            // Both user IDs reference users.AspNetUsers (cross-module — no EF FK constraint)
            builder.Property(e => e.FollowerId)
                .IsRequired();

            builder.Property(e => e.FolloweeId)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            // Indexes

            // B-Tree index on FolloweeId — used by the Fanout-on-Write background worker
            // to query "get all followers of this user" when a new post is created
            builder.HasIndex(e => e.FolloweeId)
                .HasDatabaseName("IX_Follows_FolloweeId");

            // B-Tree index on FollowerId — used to list "who does this user follow"
            builder.HasIndex(e => e.FollowerId)
                .HasDatabaseName("IX_Follows_FollowerId");
        }
    }
}
