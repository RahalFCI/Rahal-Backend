using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;

namespace Users.Application.DTOs
{
    public record RegisterRequestDto(
        string Name,
        string Email,
        string PhoneNumber,
        DateOnly BirthDate,
        GenderEnum gender,
        string Bio,
        string CountryCode,
        bool IsPublic,
        string ProfilePictureUrl)
    {
        public RegisterRequestDto() : this(string.Empty, string.Empty, string.Empty, default, default, string.Empty, string.Empty, default, string.Empty)
        {
        }
    }
}
