using Microsoft.EntityFrameworkCore;
using Places.Domain.Entities;
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
        
        public DbSet<Place> Place { get; set; }
        public DbSet<PlaceCategory> PlaceCategory { get; set; }
        public DbSet<PlacePhoto> PlacePhoto { get; set; }
        public DbSet<PlaceReview> PlaceReview { get; set; }
        public DbSet<CheckIn> CheckIn { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("places");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlacesDbContext).Assembly);
        }
    }
}
