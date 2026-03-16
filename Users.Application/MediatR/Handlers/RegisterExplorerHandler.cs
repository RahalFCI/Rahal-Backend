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
    public class RegisterExplorerHandler : IRequestHandler<RegisterExplorerRequest, User>
    {
        public Task<User> Handle(RegisterExplorerRequest request, CancellationToken cancellationToken)
        {
            var explorer = request.Dto.ToEntity();
            return Task.FromResult<User>(explorer);
        }
    }
}
