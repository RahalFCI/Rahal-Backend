using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.DTOs
{
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }
}
