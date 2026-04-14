using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Settings
{
    public class MailSettings
    {
        public const string SectionName = "Mail";
        public required string Host { get; init; }
        public required int Port { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
        public required string FromEmail { get; init; }
        public required string FromName { get; init; }
        public bool EnableSsl { get; init; } = true;
    }
}
