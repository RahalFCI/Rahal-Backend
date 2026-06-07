using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Application.DTOs.Register;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Mappers
{
    public static class MappingExtension
    {
        // ═══════════════════════════════════════════════════════
        //  User MAPPINGS
        // ═══════════════════════════════════════════════════════


        public static User CreateUser(this BaseRegisterDto dto) => new()
        {
            DisplayName = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            UserType = dto.UserRole,
        };

        public static BaseUserDto UserToDto(this User user) => new()
        {
            Id = user.Id,
            Name = user.DisplayName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = user.UserType,
        };

    }
}
