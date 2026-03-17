using System;
using Users.Application.DTOs;
using Users.Application.DTOs.Register;
using Users.Application.Mappers;
using Users.Domain.Entities._Common;

namespace Users.Application.Factory
{
    public class ExplorerUserFactory : IUserFactory<RegisterExplorerDto, User>
    {
        public User CreateUser(RegisterExplorerDto dto)
        {
            var user = dto.CreateExplorerUser();
            user.ExplorerProfile = dto.CreateExplorerProfile(user.Id, user);
            return user;
        }
    }
}
