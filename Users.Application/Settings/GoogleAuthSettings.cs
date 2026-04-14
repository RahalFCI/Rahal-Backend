using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Users.Application.Settings
{
    public class GoogleAuthSettings
    {
        public const string SectionName = "OAuth:Google";
        public required string ClientId { get; init; }
    }
}
