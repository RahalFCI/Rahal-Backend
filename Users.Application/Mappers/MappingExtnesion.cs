using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Admin;
using Users.Application.DTOs.Explorer;
using Users.Application.DTOs.Register;
using Users.Application.DTOs.Vendor;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Application.Mappers
{
    internal static class MappingExtnesion
    {

        // ═══════════════════════════════════════════════════════
        //  EXPLORER MAPPINGS
        // ═══════════════════════════════════════════════════════
        public static ExplorerDto ToDto(this Explorer user) => new()
        {
            Id = user.Id,
            Name = user.DisplayName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!,
            BirthDate = user.BirthDate,
            gender = user.Gender,
            Bio = user.Bio,
            CountryCode = user.CountryCode,
            AvailableXp = user.AvailableXp,
            CumlativeXp = user.CumaltiveXp,
            Level = user.Level,
            IsPublic = user.IsPublic,
            IsPremium = user.IsPremium,
            ProfilePictureUrl = user.ProfilePictureURL
        };

        public static ExplorerSummaryDto ToSummaryDto(this Explorer user) => new()
        {
            Id = user.Id,
            Name = user.DisplayName,
            Bio = user.Bio,
            CumlativeXp = user.CumaltiveXp,
            IsPremium = user.IsPremium,
            IsPublic = user.IsPublic,
            Level = user.Level,
            ProfilePictureUrl = user.ProfilePictureURL
        };

        public static Explorer ToEntity(this RegisterExplorerDto dto) => new()
        {
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            BirthDate = dto.BirthDate,
            Gender = dto.Gender,
            Bio = dto.Bio,
            CountryCode = dto.CountryCode,
            IsPublic = dto.IsPublic,
            ProfilePictureURL = dto.ProfilePictureUrl,
            Role = UserRoleEnum.Explorer,
        };

        public static Explorer ToEntity(this ExplorerDto dto) => new()
        {
            Id = dto.Id,
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            BirthDate = dto.BirthDate,
            Gender = dto.gender,
            Bio = dto.Bio,
            CountryCode = dto.CountryCode,
            AvailableXp = dto.AvailableXp,
            CumaltiveXp = dto.CumlativeXp,
            Level = dto.Level,
            IsPublic = dto.IsPublic,
            IsPremium = dto.IsPremium,
            ProfilePictureURL = dto.ProfilePictureUrl,
            Role = UserRoleEnum.Explorer,
        };

        // ═══════════════════════════════════════════════════════
        //  VENDOR MAPPINGS
        // ═══════════════════════════════════════════════════════
        public static VendorDto ToDto(this Vendor vendor) => new()
        {
            Id = vendor.Id,
            Name = vendor.DisplayName,
            Email = vendor.Email!,
            PhoneNumber = vendor.PhoneNumber!,
            ProfilePictureUrl = vendor.ProfilePictureURL,
            Role = vendor.Role,
            CountryCode = vendor.CountryCode,
            Address = vendor.Address,
            AddressUrl = vendor.AddressUrl,
            WorkingHours = vendor.WorkingHours,
            CategoryId = vendor.CategoryId,
            IsApproved = vendor.IsApproved
        };

        public static VendorSummaryDto ToSummaryDto(this Vendor vendor) => new()
        {
            Id = vendor.Id,
            Name = vendor.DisplayName,
            ProfilePictureUrl = vendor.ProfilePictureURL,
            Role = vendor.Role,
            CountryCode = vendor.CountryCode,
            Address = vendor.Address,
            AddressUrl = vendor.AddressUrl,
            CategoryId = vendor.CategoryId,
            IsApproved = vendor.IsApproved
        };

        public static Vendor ToEntity(this RegisterVendorDto dto) => new()
        {
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CountryCode = dto.CountryCode,
            Address = dto.Address,
            AddressUrl = dto.AddressUrl,
            WorkingHours = dto.WorkingHours,
            CategoryId = dto.CategoryId,
            ProfilePictureURL = dto.ProfilePictureUrl,
            Role = UserRoleEnum.Vendor,
        };

        public static Vendor ToEntity(this VendorDto dto) => new()
        {
            Id = dto.Id,
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CountryCode = dto.CountryCode,
            Address = dto.Address,
            AddressUrl = dto.AddressUrl,
            WorkingHours = dto.WorkingHours,
            CategoryId = dto.CategoryId,
            ProfilePictureURL = dto.ProfilePictureUrl,
            Role = dto.Role,
            IsApproved = dto.IsApproved
        };

        // ═══════════════════════════════════════════════════════
        //  ADMIN MAPPINGS
        // ═══════════════════════════════════════════════════════
        public static AdminDto ToDto(this Admin admin) => new()
        {
            Id = admin.Id,
            Name = admin.DisplayName,
            Email = admin.Email!,
            ProfilePictureUrl = admin.ProfilePictureURL,
            Role = admin.Role,
            PhoneNumber = admin.PhoneNumber!
        };

        public static AdminSummaryDto ToSummary(this Admin admin) => new()
        {
            Id = admin.Id,
            Name = admin.DisplayName,
            Email = admin.Email!,
            ProfilePictureUrl = admin.ProfilePictureURL,
            Role = admin.Role,
            PhoneNumber = admin.PhoneNumber!
        };

        public static Admin ToEntity(this RegisterAdminDto dto) => new()
        {
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            ProfilePictureURL = dto.ProfilePictureUrl,
            Role = UserRoleEnum.Admin,
        };

        public static Admin ToEntity(this AdminDto dto) => new()
        {
            Id = dto.Id,
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            ProfilePictureURL = dto.ProfilePictureUrl,
            Role = dto.Role,
        };
    }
}
