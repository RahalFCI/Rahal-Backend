using System;
using Users.Application.DTOs;
using Users.Application.DTOs.Register;
using Users.Application.Mappers;
using Users.Domain.Entities._Common;

namespace Users.Application.Factory
{
    public class VendorUserFactory : IUserFactory<RegisterVendorDto, User>
    {
        public User CreateUser(RegisterVendorDto dto)
        {
            var user = dto.CreateVendorUser();
            user.VendorProfile = dto.CreateVendorProfile(user.Id, user);
            return user;
        }
    }
}
