using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Settings
{
    public class RedisSettings
    {
        public bool AbortOnConnectFail { get; set; }
        public int ConnectRetry { get; set; }
        public int ConnectTimeout { get; set; }
    }
}
