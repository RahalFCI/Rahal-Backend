using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMedia.Infrastructure.Persistence
{
    public class SocialMediaDbContext : DbContext
    {
        public SocialMediaDbContext(DbContextOptions<SocialMediaDbContext> options) : base(options)
        {
        }
        //TODO: DbSets to be added and make its configuration file
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("socialmedia");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SocialMediaDbContext).Assembly);
        }
    }
}
