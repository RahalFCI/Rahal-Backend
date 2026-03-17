using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configuration
{

    public class AdminProfileConfiguration : IEntityTypeConfiguration<AdminProfile>
    {
        public void Configure(EntityTypeBuilder<AdminProfile> builder)
        {
            builder.ToTable("AdminProfiles", "users");

            builder.HasKey(e => e.Id);

            // Foreign key to User
            builder.HasOne(e => e.User)
                .WithOne(u => u.AdminProfile)
                .HasForeignKey<AdminProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            builder.HasIndex(e => e.UserId)
                .IsUnique();
        }
    }
}
