using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities._Common;

namespace Users.Application.Factory
{
    public interface IUserFactory<TDto, TUser> where TUser : User
    {
        Task<TUser> CreateUser(TDto dto);
    }
}
