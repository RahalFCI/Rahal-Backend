using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Places.Domain.Entities
{
    public class PlaceCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public IEnumerable<Place> Places { get; set; } = new List<Place>();
    }
}
