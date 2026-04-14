using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs.OAuth
{
    public record GoogleUserInfo(
    string Email,
    string FirstName,
    string LastName,
    string? PictureUrl,
    string GoogleId);    // stable unique ID from Google ("sub" claim)

}
