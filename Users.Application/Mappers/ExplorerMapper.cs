using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs.Explorer;
using Users.Application.Interfaces;
using Users.Domain.Entities;

namespace Users.Application.Mappers
{
    internal class ExplorerMapper : IUserMapper<Explorer, ExplorerDto, ExplorerSummaryDto>
    {
        public ExplorerDto ToDto(Explorer user) => user.ToDto();  
        public ExplorerSummaryDto ToSummary(Explorer user) => user.ToSummaryDto();
        public Explorer ToEntity(ExplorerDto dto) => dto.ToEntity();
    }
}
