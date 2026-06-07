using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Domain.Entities
{
    public class Badge : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
