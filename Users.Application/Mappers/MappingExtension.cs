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
    internal static class MappingExtension
    {
        // ═══════════════════════════════════════════════════════
        //  EXPLORER MAPPINGS
        // ═══════════════════════════════════════════════════════


        public static User CreateExplorerUser(this RegisterExplorerDto dto) => new()
        {
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            UserType = UserRoleEnum.Explorer,
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

    }
}
