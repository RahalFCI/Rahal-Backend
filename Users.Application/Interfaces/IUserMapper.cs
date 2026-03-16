 using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.Interfaces
{
    public interface IUserMapper<TUser, TDto, TSummary>
        where TUser : class
        where TDto : class
        where TSummary : class
    {
        TSummary ToSummary(TUser user);
        TDto ToDto(TUser user);
        TUser ToEntity(TDto dto);
    }
}
