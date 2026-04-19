using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs.Register
{
    public record RegisterVendorDto(
        string Name,
        string Email,
        string Password,
        string ConfirmPassword,
        string PhoneNumber,
        string CountryCode,
        string Address,
        string AddressUrl,
        Dictionary<DayOfWeek, string> WorkingHours,
        Guid CategoryId)
    {
        public RegisterVendorDto() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, new Dictionary<DayOfWeek, string>(), Guid.Empty)
        {
        }
    }
}
