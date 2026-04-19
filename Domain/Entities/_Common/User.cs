using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;

namespace Users.Domain.Entities._Common
{
    /// <summary>
    /// Base User class for Identity authentication
    /// Contains only common properties. Type-specific data stored in profile tables.
    /// </summary>
    public class User : IdentityUser<Guid>
    {

        public string DisplayName { get; set; } = string.Empty;


        public string ProfilePictureURL { get; set; } = string.Empty;

        public UserRoleEnum UserType { get; set; }


        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; } = DateTime.MinValue;

        // OTP fields for password reset
        public string? PasswordResetOtp { get; set; }

        public DateTime? PasswordResetOtpExpiry { get; set; }

        public ExplorerProfile? ExplorerProfile { get; set; }


        public VendorProfile? VendorProfile { get; set; }


        public AdminProfile? AdminProfile { get; set; }
    }
}

