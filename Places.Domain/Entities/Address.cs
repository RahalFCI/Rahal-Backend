using System;
using System.Collections.Generic;
using System.Text;

namespace Places.Domain.Entities
{
    public class Address
    {
        public string AddressLine { get; set; } = string.Empty;
        public string Government { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
