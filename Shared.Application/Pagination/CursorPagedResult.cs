using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Pagination
{
    public class CursorPagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public string? NextCursor { get; set; } // null means no more data
        public bool HasNextPage => NextCursor is not null;
    }
}
