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
        /// <summary>
        /// User's display name
        /// </summary>
        public required string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// User's profile picture URL
        /// </summary>
        public string ProfilePictureURL { get; set; } = string.Empty;

        /// <summary>
        /// User type (Admin, Vendor, Explorer)
        /// </summary>
        public required UserRoleEnum UserType { get; set; }

        /// <summary>
        /// Refresh token for JWT
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Refresh token expiry time
        /// </summary>
        public DateTime? RefreshTokenExpiryTime { get; set; }

        /// <summary>
        /// Navigation to Explorer profile (if UserType == Explorer)
        /// </summary>
        public ExplorerProfile? ExplorerProfile { get; set; }

        /// <summary>
        /// Navigation to Vendor profile (if UserType == Vendor)
        /// </summary>
        public VendorProfile? VendorProfile { get; set; }

        /// <summary>
        /// Navigation to Admin profile (if UserType == Admin)
        /// </summary>
        public AdminProfile? AdminProfile { get; set; }
    }
}
