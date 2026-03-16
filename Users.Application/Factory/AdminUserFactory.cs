using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs.Register;
using Users.Application.Mappers;
using Users.Domain.Entities;

namespace Users.Application.Factory
{
    public class AdminUserFactory : IUserFactory<RegisterAdminDto, Admin>
    {
        public Admin CreateUser(RegisterAdminDto dto)
        {
            return dto.ToEntity();
        }
    }
}
