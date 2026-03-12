using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.DTOs
{
    public record ApiResponse<TResult>(TResult Data, bool IsSuccess, ErrorCode errorCode) { 
        public static ApiResponse<TResult> Success(TResult data) => new(data, true, ErrorCode.None);
            public static ApiResponse<TResult> Failure(ErrorCode errorCode) => new(default!, false, errorCode);

    }
}
