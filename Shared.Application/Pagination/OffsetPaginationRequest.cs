using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Pagination
{
    public class OffsetPaginationRequest : PaginationRequest
    {
        public int Page { get; set; } = 1;
    }
}
