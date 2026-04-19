using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Pagination
{
    public abstract class PaginationRequest
    {
        public int PageSize { get; set; } = 10;
    }
}
