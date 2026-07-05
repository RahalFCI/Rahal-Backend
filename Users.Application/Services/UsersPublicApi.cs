using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Users.Contracts.DTOs;
using Users.Contracts.Interfaces;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

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
                .Where(u => u.UserType == UserRoleEnum.Explorer)
                .Where(u => ids.Contains(u.Id))
                .Select(u => new UserPublicDto
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName
                })
                .ToListAsync(cancellationToken);

            return users;
        }

        public async Task<UserPublicPagedResult> GetUsersPaginatedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _userManager.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserRoleEnum.Explorer)
                .OrderBy(u => u.DisplayName)
                .ThenBy(u => u.Id);

            var totalCount = await query.CountAsync(cancellationToken);
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserPublicDto
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName
                })
                .ToListAsync(cancellationToken);

            return new UserPublicPagedResult
            {
                Items = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
