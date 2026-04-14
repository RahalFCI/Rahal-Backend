using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Entities
{
    public record MailAttachment(string FileName, byte[] Content, string ContentType);

}
