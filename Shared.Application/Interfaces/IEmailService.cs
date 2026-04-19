using Shared.Application.DTOs.Mail;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(MailRequest request, CancellationToken ct = default);
    }
}
