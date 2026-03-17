using System;
using Users.Application.DTOs;
using Users.Application.DTOs.Register;
using Users.Application.Mappers;
using Users.Domain.Entities._Common;

namespace Users.Application.Factory
{
    public class AdminUserFactory : IUserFactory<RegisterAdminDto, User>
    {
        public User CreateUser(RegisterAdminDto dto)
        {
            var user = dto.CreateAdminUser();
            user.AdminProfile = dto.CreateAdminProfile(user.Id, user);
            return user;
        }
    }
}
