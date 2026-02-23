using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Places.Infrastructure.Persistence
{
    public class PlacesDbContext : DbContext
    {
        public PlacesDbContext(DbContextOptions<PlacesDbContext> options) : base(options)
        {
        }
        //TODO: DbSets to be added and make its configuration file
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("places");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlacesDbContext).Assembly);
        }
    }
}
