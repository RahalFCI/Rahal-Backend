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
    public class RegisterAdminHandler : IRequestHandler<RegisterAdminRequest, User>
    {
        public Task<User> Handle(RegisterAdminRequest request, CancellationToken cancellationToken)
        {
            var admin = request.Dto.ToEntity();
            return Task.FromResult<User>(admin);
        }
    }
}
