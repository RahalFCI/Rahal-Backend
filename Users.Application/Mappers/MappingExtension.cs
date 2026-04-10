using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Admin;
using Users.Application.DTOs.Explorer;
using Users.Application.DTOs.Register;
using Users.Application.DTOs.Vendor;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Mappers
{
    /// <summary>
    /// Extension methods for mapping between DTOs and entities
    /// Handles creation of User + Profile pairs from registration DTOs
    /// </summary>
    internal static class MappingExtension
    {
        // ═══════════════════════════════════════════════════════
        //  EXPLORER MAPPINGS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Create User entity from Explorer registration DTO
        /// Profile is created separately in factory
        /// </summary>
        public static User CreateExplorerUser(this RegisterExplorerDto dto) => new()
        {
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            UserType = UserRoleEnum.Explorer,
        };

        /// <summary>
        /// Create ExplorerProfile from registration DTO
        /// Called after User is created to get UserId
        /// </summary>
        public static ExplorerProfile CreateExplorerProfile(this RegisterExplorerDto dto, Guid userId, User user) => new()
        {
            UserId = userId,
            User = user,
            Gender = dto.Gender,
            BirthDate = dto.BirthDate,
            Bio = dto.Bio,
            CountryCode = dto.CountryCode,
            IsPublic = dto.IsPublic,
        };

        // ═══════════════════════════════════════════════════════
        //  VENDOR MAPPINGS
        // ═══════════════════════════════════════════════════════

        public static User CreateVendorUser(this RegisterVendorDto dto) => new()
        {
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            UserType = UserRoleEnum.Vendor,
        };

        public static VendorProfile CreateVendorProfile(this RegisterVendorDto dto, Guid userId, User user) => new()
        {
            UserId = userId,
            User = user,
            CountryCode = dto.CountryCode,
            Address = dto.Address,
            AddressUrl = dto.AddressUrl,
            WorkingHours = dto.WorkingHours,
            CategoryId = dto.CategoryId,
        };

        // ═══════════════════════════════════════════════════════
        //  ADMIN MAPPINGS
        // ═══════════════════════════════════════════════════════

        public static User CreateAdminUser(this RegisterAdminDto dto) => new()
        {
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            UserType = UserRoleEnum.Admin,
        };

        public static AdminProfile CreateAdminProfile(this RegisterAdminDto dto, Guid userId, User user) => new()
        {
            UserId = userId,
            User = user,
        };
    }
}
