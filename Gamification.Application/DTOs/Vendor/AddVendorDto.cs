using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;


namespace Users.Application.DTOs.Vendor
{
    public record AddVendorDto(
        Guid UserId,
        string CountryCode,
        string Address,
        string AddressUrl,
        Dictionary<DayOfWeek, string> WorkingHours,
        Guid CategoryId)
    {
        public AddVendorDto() : this(default, string.Empty, string.Empty, string.Empty, new Dictionary<DayOfWeek, string>(), default) { }
    }
}
