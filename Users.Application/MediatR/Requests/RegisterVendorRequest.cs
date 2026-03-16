using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Register;
using Users.Domain.Entities._Common;

namespace Users.Application.MediatR.Requests
{
    public record RegisterVendorRequest(RegisterVendorDto Dto) : IRequest<User>;
}
