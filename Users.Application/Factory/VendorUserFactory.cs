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
    public class VendorUserFactory : IUserFactory<RegisterVendorDto, User>
    {

        private readonly IMediator _mediatR;

        public VendorUserFactory(IMediator mediatR)
        {
            _mediatR = mediatR;
        }
        public async Task<User> CreateUser(RegisterVendorDto dto)
        {
            var user = dto.CreateVendorUser();

            var profileCreatedEvent = new VendorProfileCreatedEvent(
                UserId: user.Id,
                CountryCode: dto.CountryCode,
                Address: dto.Address,
                AddressUrl: dto.AddressUrl,
                WorkingHours: dto.WorkingHours,
                CategoryId: dto.CategoryId);

            await _mediatR.Publish(profileCreatedEvent);

            return user;
        }
    }
}
