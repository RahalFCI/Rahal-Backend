using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.Mappers;
using Users.Application.MediatR.Requests;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;

namespace Users.Application.MediatR.Handlers
{
    public class RegisterVendorHandler : IRequestHandler<RegisterVendorRequest, User>
    {
        public Task<User> Handle(RegisterVendorRequest request, CancellationToken cancellationToken)
        {
            var vendor = request.Dto.ToEntity();
            return Task.FromResult<User>(vendor);
        }
    }
}
