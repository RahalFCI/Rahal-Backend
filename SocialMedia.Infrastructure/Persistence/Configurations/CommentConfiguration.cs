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

            // IMPORTANT: No global HasQueryFilter on IsDeleted.
            // We filter explicitly in LINQ so deleted parents are still visible
            // in thread context when they have active replies.

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

            builder.Property(e => e.RepliesCount)
                .HasDefaultValue(0)
                .IsRequired();

            // FK to Post
            builder.HasOne(e => e.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Self-referencing FK for nested replies
            // DeleteBehavior.Restrict prevents cascade delete cycles on the self-referencing path.
            // We rely on soft-delete, not hard cascade, for comments.
            builder.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Composite Index without partial filter — deleted comments stay so their
            // child threads remain intact (Reddit-style soft delete).
            builder.HasIndex(e => new { e.PostId, e.ParentCommentId })
                .HasDatabaseName("IX_Comments_PostId_ParentCommentId");

            // B-Tree Index on ParentCommentId for fetching nested replies
            builder.HasIndex(e => e.ParentCommentId)
                .HasDatabaseName("IX_Comments_ParentCommentId");
        }
    }
}
