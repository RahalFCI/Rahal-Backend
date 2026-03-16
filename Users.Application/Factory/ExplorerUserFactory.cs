using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs.Register;
using Users.Application.Mappers;
using Users.Domain.Entities;

namespace Users.Application.Factory
{
    public class ExplorerUserFactory : IUserFactory<RegisterExplorerDto, Explorer>
    {
        public Explorer CreateUser(RegisterExplorerDto dto)
        {
            return dto.ToEntity();
        }
    }
}
