using System;
using MediatR;
using Shared.Domain.Entities;
using Shared.Domain.Events;
using Users.Application.DTOs;
using Users.Application.DTOs.Register;
using Users.Application.Mappers;
using Users.Domain.Entities._Common;
using Users.Domain.Events;

namespace Users.Application.Factory
{
    public class ExplorerUserFactory : IUserFactory<RegisterExplorerDto, User>
    {
        private readonly IMediator _mediatR;

        public ExplorerUserFactory(IMediator mediatR)
        {
            _mediatR = mediatR;
        }

        async Task<User> IUserFactory<RegisterExplorerDto, User>.CreateUser(RegisterExplorerDto dto)
        {
            var user = dto.CreateExplorerUser();

            var profileCreatedEvent = new ExplorerProfileCreatedEvent(
                UserId: user.Id,
                Gender: dto.Gender,
                BirthDate: dto.BirthDate,
                Bio: dto.Bio,
                CountryCode: dto.CountryCode);

            await _mediatR.Publish(profileCreatedEvent);

            return user;
        }
    }
}
