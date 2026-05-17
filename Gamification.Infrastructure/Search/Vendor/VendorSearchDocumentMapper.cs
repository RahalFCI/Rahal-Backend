using Gamification.Domain.Entities;
using Gamification.Infrastructure.Search.Explorer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Search.Vendor
{
    public static class VendorSearchDocumentMapper
    {
        public static VendorSearchDocument ToSearchDocument(this VendorProfile user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new VendorSearchDocument
            {
                UserId = user.Id,
                Name = user.DisplayName,
                ProfilePictureUrl = user.ProfilePictureURL
            };
        }
        public static IEnumerable<VendorSearchDocument> ToSearchDocuments(this IEnumerable<VendorProfile> users)
        {
            return users?.Select(u => u.ToSearchDocument()) ?? Enumerable.Empty<VendorSearchDocument>();
        }
    }
}
