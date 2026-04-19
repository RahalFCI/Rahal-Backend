using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Pagination
{
    public class CursorPaginationRequest : PaginationRequest
    {
        public string? Cursor { get; set; } // last seen id
    }
}
