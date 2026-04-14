using Users.Domain.Entities._Common;

namespace Users.Infrastructure.Search
{
    public static class UserSearchDocumentMapper
    {
        public static UserSearchDocument ToSearchDocument(this User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new UserSearchDocument
            {
                Id = user.Id.ToString(),
                Username = user.UserName ?? string.Empty,
                FullName = user.DisplayName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                IsVerified = user.EmailConfirmed,
                ProfilePictureUrl = user.ProfilePictureURL ?? string.Empty
            };
        }
        public static IEnumerable<UserSearchDocument> ToSearchDocuments(this IEnumerable<User> users)
        {
            return users?.Select(u => u.ToSearchDocument()) ?? Enumerable.Empty<UserSearchDocument>();
        }
    }
}
