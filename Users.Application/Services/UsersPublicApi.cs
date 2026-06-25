using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Users.Contracts.DTOs;
using Users.Contracts.Interfaces;
using Users.Domain.Entities._Common;

namespace Users.Application.Services
{
    public class UsersPublicApi : IUsersPublicApi
    {
        private readonly UserManager<User> _userManager;

        public UsersPublicApi(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserPublicDto>> GetUsersDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return Enumerable.Empty<UserPublicDto>();

            var users = await _userManager.Users
                .Where(u => ids.Contains(u.Id))
                .Select(u => new UserPublicDto
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName
                })
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}
