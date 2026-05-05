 using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{

    public interface IUserMapper<TDto, TSummary>
        where TDto : BaseUserDto
        where TSummary : BaseUserSummaryDto
    {
        TSummary ToSummary(User user);

        TDto ToDto(User user);
    }
}

