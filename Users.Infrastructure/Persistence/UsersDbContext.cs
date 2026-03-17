using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;

namespace Users.Infrastructure.Persistence
{

    public class UsersDbContext : IdentityDbContext<User, Role, Guid>
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
        {
        }



        public DbSet<ExplorerProfile> ExplorerProfiles { get; set; }

        public DbSet<VendorProfile> VendorProfiles { get; set; }


        public DbSet<AdminProfile> AdminProfiles { get; set; }


        public DbSet<VendorCategory> VendorCategories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("users");

            // Apply all entity configurations from the assembly
            // Entity configurations are defined in separate files for maintainability
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
        }
    }
}

