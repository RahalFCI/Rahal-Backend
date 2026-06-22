using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Persistence.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments", "socialmedia");

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

            builder.Property(e => e.PostId)
                .IsRequired();

            // UserId references users.AspNetUsers (cross-module — no EF FK constraint)
            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.ParentCommentId);

            builder.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("text");

            // FK to Post
            builder.HasOne(e => e.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Self-referencing FK for nested replies
            // DeleteBehavior.Restrict prevents cascade delete cycles on the self-referencing path.
            // Parent post deletion cascades through Post → Comments (handled above);
            // this FK only governs direct parent-child comment deletion.
            builder.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes

            // Composite index: fetch root comments for a post (ParentCommentId IS NULL)
            // and all comments for a post ordered by creation — covers both use cases
            builder.HasIndex(e => new { e.PostId, e.ParentCommentId })
                .HasDatabaseName("IX_Comments_PostId_ParentCommentId");

            // B-Tree index on ParentCommentId — fetch nested replies for a specific comment
            builder.HasIndex(e => e.ParentCommentId)
                .HasDatabaseName("IX_Comments_ParentCommentId");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Comments_IsDeleted");
        }
    }
}
