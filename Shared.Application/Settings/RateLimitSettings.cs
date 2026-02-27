using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Settings
{
    public class RateLimitSettings
    {
        public int RequestLimit { get; set; } = 100;
        public int WindowInSeconds { get; set; } = 60;
        public bool EnableIpRateLimit { get; set; } = true;
        public bool EnableUserRateLimit { get; set; } = true;
    }
}
}
