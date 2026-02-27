using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Settings
{
    public class JWTSettings
    {
        public string? Secret { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int AccessTokenExpiryInMin { get; set; }
        public int RefreshTokenExpiryInDay { get; set; }
    }
}
