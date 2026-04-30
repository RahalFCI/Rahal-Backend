using Microsoft.EntityFrameworkCore;
using Shared.Application.Pagination;
using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Pagination
{
    public static class PaginationExtensions
    {
        // offset
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            OffsetPaginationRequest request,
            CancellationToken ct = default)
        {
            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        // cursor
        public static async Task<CursorPagedResult<T>> ToCursorPagedResultAsync<T>(
            this IQueryable<T> query,
            CursorPaginationRequest request,
            Func<T, string> cursorSelector,  // tells us which field is the cursor
            CancellationToken ct = default)
            where T : BaseEntity
        {
            // decode cursor and apply it
            if (request.Cursor is not null)
            {
                var decodedCursor = DecodeCursor(request.Cursor);
                query = query.Where(e => e.CreatedAt < decodedCursor);
            }

            // take one extra to know if there's a next page
            var items = await query
                .OrderByDescending(e => e.CreatedAt)
                .Take(request.PageSize + 1)
                .ToListAsync(ct);

            var hasNextPage = items.Count > request.PageSize;

            if (hasNextPage)
                items.RemoveAt(items.Count - 1); // remove the extra item

            return new CursorPagedResult<T>
            {
                Items = items,
                NextCursor = hasNextPage
                    ? EncodeCursor(items.Last().CreatedAt)
                    : null
            };
        }

        // encode/decode cursor to hide implementation details from client
        private static string EncodeCursor(DateTime createdAt)
            => Convert.ToBase64String(
                Encoding.UTF8.GetBytes(createdAt.ToString("O")));

        private static DateTime DecodeCursor(string cursor)
            => DateTime.Parse(
                Encoding.UTF8.GetString(Convert.FromBase64String(cursor)),
                null,
                System.Globalization.DateTimeStyles.RoundtripKind);
    }
}
