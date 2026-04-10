using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;

namespace Users.Application.DTOs.Register
{
    public record RegisterExplorerDto(
        string Name,
        string Email,
        string Password,
        string ConfirmPassword,
        string PhoneNumber,
        DateOnly BirthDate,
        GenderEnum Gender,
        string Bio,
        string CountryCode,
        bool IsPublic)
    {
        public RegisterExplorerDto() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, default, default, string.Empty, string.Empty, default)
        {
        }
    }
}
