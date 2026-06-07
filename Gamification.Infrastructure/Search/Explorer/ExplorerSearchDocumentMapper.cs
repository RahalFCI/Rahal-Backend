using Gamification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities._Common;

namespace Gamification.Infrastructure.Search.Explorer
{
    public static class ExplorerSearchDocumentMapper
    {
        public static ExplorerSearchDocument ToSearchDocument(this ExplorerProfile user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new ExplorerSearchDocument
            {
                UserId = user.Id,
                Name = user.DisplayName,
                ProfilePictureUrl = user.ProfilePictureURL
            };
        }
        public static IEnumerable<ExplorerSearchDocument> ToSearchDocuments(this IEnumerable<ExplorerProfile> users)
        {
            return users?.Select(u => u.ToSearchDocument()) ?? Enumerable.Empty<ExplorerSearchDocument>();
        }
    }
}
