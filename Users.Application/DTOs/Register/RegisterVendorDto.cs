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
        int CategoryId,
        string ProfilePictureUrl)
    {
        public RegisterVendorDto() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, new Dictionary<DayOfWeek, string>(), 0, string.Empty)
        {
        }
    }
}
