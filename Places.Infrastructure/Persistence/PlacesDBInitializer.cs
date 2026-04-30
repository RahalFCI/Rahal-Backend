using Microsoft.EntityFrameworkCore;
using Places.Domain.Entities;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Places.Infrastructure.Persistence
{
    internal class PlacesDBInitializer : IDbInitializer
    {
        private readonly PlacesDbContext _context;

        public PlacesDBInitializer(PlacesDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.PlaceCategory.AnyAsync()) return;

            _context.PlaceCategory.AddRange(
                new PlaceCategory { Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"), Name = "Historical Sites" },
                new PlaceCategory { Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"), Name = "Museums" },
                new PlaceCategory { Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"), Name = "Religious Sites" },
                new PlaceCategory { Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"), Name = "Beaches" },
                new PlaceCategory { Id = Guid.Parse("a5555555-5555-5555-5555-555555555555"), Name = "Mountains" },
                new PlaceCategory { Id = Guid.Parse("a6666666-6666-6666-6666-666666666666"), Name = "Parks and Gardens" },
                new PlaceCategory { Id = Guid.Parse("a7777777-7777-7777-7777-777777777777"), Name = "Natural Wonders" },
                new PlaceCategory { Id = Guid.Parse("a8888888-8888-8888-8888-888888888888"), Name = "Hotels" },
                new PlaceCategory { Id = Guid.Parse("a9999999-9999-9999-9999-999999999999"), Name = "Restaurants" },
                new PlaceCategory { Id = Guid.Parse("b1111111-1111-1111-1111-111111111111"), Name = "Cafes" },
                new PlaceCategory { Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"), Name = "Shopping Malls" },
                new PlaceCategory { Id = Guid.Parse("b3333333-3333-3333-3333-333333333333"), Name = "Markets" },
                new PlaceCategory { Id = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Name = "Nightlife" },
                new PlaceCategory { Id = Guid.Parse("b5555555-5555-5555-5555-555555555555"), Name = "Entertainment Venues" },
                new PlaceCategory { Id = Guid.Parse("b6666666-6666-6666-6666-666666666666"), Name = "Adventure Activities" },
                new PlaceCategory { Id = Guid.Parse("b7777777-7777-7777-7777-777777777777"), Name = "Cultural Centers" },
                new PlaceCategory { Id = Guid.Parse("b8888888-8888-8888-8888-888888888888"), Name = "Landmarks" },
                new PlaceCategory { Id = Guid.Parse("b9999999-9999-9999-9999-999999999999"), Name = "Islands" },
                new PlaceCategory { Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"), Name = "Resorts" },
                new PlaceCategory { Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"), Name = "Zoos and Aquariums" },
                new PlaceCategory { Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"), Name = "Amusement Parks" },
                new PlaceCategory { Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"), Name = "Local Experiences" },
                new PlaceCategory { Id = Guid.Parse("c5555555-5555-5555-5555-555555555555"), Name = "Viewpoints" }

            );

            await _context.SaveChangesAsync();
        }
    }
}
