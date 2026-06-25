using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Users.Contracts.DTOs;

namespace Users.Contracts.Interfaces
{
    public interface IUsersPublicApi
    {
        Task<IEnumerable<UserPublicDto>> GetUsersDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
