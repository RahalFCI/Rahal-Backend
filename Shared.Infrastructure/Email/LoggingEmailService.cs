using Microsoft.Extensions.Logging;
using Shared.Application.DTOs.Mail;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Email
{
    internal class LoggingEmailService : IEmailService
    {
        private readonly ILogger<LoggingEmailService> _logger;

        public LoggingEmailService(ILogger<LoggingEmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendAsync(MailRequest request, CancellationToken ct = default)
        {
            try
            {
                ValidateRequest(request);

                _logger.LogInformation("Email logged (not sent) - To: {RecipientEmail}", request.To);
                _logger.LogInformation("Subject: {Subject}", request.Subject);
                _logger.LogInformation("From: {DisplayName}", request.DisplayName ?? "Rahal App");

                if (!string.IsNullOrWhiteSpace(request.ReplyTo))
                {
                    _logger.LogInformation("Reply-To: {ReplyTo}", request.ReplyTo);
                }

                if (request.Cc?.Count > 0)
                {
                    var ccList = string.Join(", ", request.Cc);
                    _logger.LogInformation("CC: {CcList}", ccList);
                }

                _logger.LogInformation("Body: {Body}", request.Body);

                if (request.Attachments?.Count > 0)
                {
                    _logger.LogInformation("Attachments count: {AttachmentCount}", request.Attachments.Count);
                    foreach (var attachment in request.Attachments)
                    {
                        if (!string.IsNullOrWhiteSpace(attachment.FileName))
                        {
                            _logger.LogInformation("Attachment: {FileName} ({ContentType})", attachment.FileName, attachment.ContentType);
                        }
                    }
                }

                _logger.LogInformation("Email logged successfully for {RecipientEmail}", request.To);

                await Task.CompletedTask;
            }
            catch (ArgumentNullException nullEx)
            {
                _logger.LogError(nullEx, "Null email request: {Message}", nullEx.Message);
                throw;
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Invalid email request: {Message}", argEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while logging email");
                throw;
            }
        }

        private void ValidateRequest(MailRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Mail request cannot be null");
            }

            if (string.IsNullOrWhiteSpace(request.To))
            {
                throw new ArgumentException("Recipient email address is required", nameof(request.To));
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                throw new ArgumentException("Email subject is required", nameof(request.Subject));
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                throw new ArgumentException("Email body is required", nameof(request.Body));
            }
        }
    }
}
