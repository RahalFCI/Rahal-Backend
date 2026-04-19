using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Domain.Enums;

namespace Users.Application.DTOs.Vendor
{
    public record VendorDto(
        string CountryCode,
        string Address,
        string AddressUrl,
        Dictionary<DayOfWeek, string> WorkingHours,
        Guid CategoryId,
        bool IsApproved) : BaseUserDto
    {
        public VendorDto() : this(string.Empty, string.Empty, string.Empty, new Dictionary<DayOfWeek, string>(), default, default) { }
    }
}
