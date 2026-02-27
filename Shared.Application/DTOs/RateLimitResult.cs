using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.DTOs
{
    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public int CurrentCount { get; set; }
        public DateTime WindowResetTime { get; set; }
        public int RetryAfterSeconds { get; set; }
    }
}
