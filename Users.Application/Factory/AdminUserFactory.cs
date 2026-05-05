using MediatR;
using Shared.Domain.Entities;
using Shared.Domain.Events;
using System;
using Users.Application.DTOs;
using Users.Application.DTOs.Register;
using Users.Application.Mappers;
using Users.Domain.Entities._Common;
using Users.Domain.Events;

namespace Users.Application.Factory
{
    public class AdminUserFactory : IUserFactory<RegisterAdminDto, User>
    {
        private readonly IMediator _mediatR;

        public AdminUserFactory(IMediator mediatR)
        {
            _mediatR = mediatR;
        }
        public async Task<User> CreateUser(RegisterAdminDto dto)
        {
            var user = dto.CreateAdminUser();

            var profileCreatedEvent = new AdminProfileCreatedEvent(UserId: user.Id);

            await _mediatR.Publish(profileCreatedEvent);

            return user;
        }
    }
}
