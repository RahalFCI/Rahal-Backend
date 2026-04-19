using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.DTOs.Mail
{
    public record MailAttachment(string FileName, byte[] Content, string ContentType);

}
