using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.DTOs.Vendor
{
    public record GetVendorDto(
        Guid UserId,
        string CountryCode,
        string Address,
        string AddressUrl,
        Dictionary<DayOfWeek, string> WorkingHours,
        Guid CategoryId,
        bool IsApproved)
    {
        public GetVendorDto() : this(default, string.Empty, string.Empty, string.Empty, new Dictionary<DayOfWeek, string>(), default, default) { }
    }
}
