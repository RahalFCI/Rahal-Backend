using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.DTOs.Vendor
{
    public record UpdateVendorDto(
        Guid UserId,
        string DisplayName,
        string CountryCode,
        string Address,
        string AddressUrl,
        Dictionary<DayOfWeek, string> WorkingHours,
        Guid CategoryId)
    {
        public UpdateVendorDto() : this(default, string.Empty, string.Empty, string.Empty, string.Empty, new Dictionary<DayOfWeek, string>(), default) { }
    }
}
