using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;

namespace Users.Domain.Entities._Common
{
    public abstract class User : IdentityUser<Guid>
    {
        public required string DisplayName { get; set; } = string.Empty;
        public string ProfilePictureURL { get; set; } = string.Empty;
        public required UserRoleEnum Role { get; set; }

    }
}
