using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs.Admin;
using Users.Application.DTOs.Vendor;
using Users.Application.Interfaces;
using Users.Domain.Entities;

namespace Users.Application.Mappers
{
    internal class AdminMapper : IUserMapper<Admin, AdminDto, AdminSummaryDto>
    {
        public AdminDto ToDto(Admin user) => user.ToDto();
        public AdminSummaryDto ToSummary(Admin user) => user.ToSummary();
        public Admin ToEntity(AdminDto dto) => dto.ToEntity();
    }
}
