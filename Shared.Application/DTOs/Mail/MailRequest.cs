using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.DTOs.Mail
{
    public record MailRequest
    {
        public required string To { get; init; }
        public required string Subject { get; init; }
        public required string Body { get; init; }   // HTML
        public string? DisplayName { get; init; }   // "Rahal App" shown in email client
        public string? ReplyTo { get; init; }
        public IList<string> Cc { get; init; } = [];
        public IList<MailAttachment> Attachments { get; init; } = [];
    }
}
