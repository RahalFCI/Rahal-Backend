using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Settings;
using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Shared.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly MailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly ResiliencePipeline _resiliencePipeline;


        public SmtpEmailService(IOptions<MailSettings> settings, ILogger<SmtpEmailService> logger, ResiliencePipelineProvider<string> resiliencePipeline)
        {
            _settings = settings.Value;
            _logger = logger;
            _resiliencePipeline = resiliencePipeline.GetPipeline("email");
        }

        public async Task SendAsync(MailRequest request, CancellationToken ct = default)
        {
            try
            {
                ValidateRequest(request);

                using (var client = CreateSmtpClient())
                {
                    _logger.LogInformation("Connecting to SMTP server {SmtpHost}:{SmtpPort}", _settings.Host, _settings.Port);

                    using (var mailMessage = BuildMailMessage(request))
                    {
                        _logger.LogInformation("Sending email to {RecipientEmail} with subject '{Subject}'", request.To, request.Subject);

                        await client.SendMailAsync(mailMessage, ct);

                        _logger.LogInformation("Email successfully sent to {RecipientEmail}", request.To);
                    }
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error occurred while sending email to {RecipientEmail}. Status code: {StatusCode}", 
                    request.To, smtpEx.StatusCode);
                throw;
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Invalid email address provided: {RecipientEmail}", request.To);
                throw;
            }
            catch (OperationCanceledException cancelEx)
            {
                _logger.LogWarning(cancelEx, "Email sending operation was cancelled for {RecipientEmail}", request.To);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while sending email to {RecipientEmail}", request.To);
                throw;
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient
            {
                Host = _settings.Host,
                Port = _settings.Port,
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            return client;
        }

        private MailMessage BuildMailMessage(MailRequest request)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = true
            };

            // Add recipient
            mailMessage.To.Add(new MailAddress(request.To, request.DisplayName ?? string.Empty));

            // Add reply-to if specified
            if (!string.IsNullOrWhiteSpace(request.ReplyTo))
            {
                mailMessage.ReplyToList.Add(new MailAddress(request.ReplyTo));
            }

            // Add CC recipients
            if (request.Cc?.Count > 0)
            {
                foreach (var ccEmail in request.Cc)
                {
                    if (!string.IsNullOrWhiteSpace(ccEmail))
                    {
                        mailMessage.CC.Add(new MailAddress(ccEmail));
                    }
                }
            }

            // Add attachments
            if (request.Attachments?.Count > 0)
            {
                foreach (var attachment in request.Attachments)
                {
                    if (!string.IsNullOrWhiteSpace(attachment.FileName) && 
                        attachment.Content?.Length > 0)
                    {
                        var stream = new MemoryStream(attachment.Content);
                        var mailAttachment = new System.Net.Mail.Attachment(stream, attachment.FileName, attachment.ContentType);
                        mailMessage.Attachments.Add(mailAttachment);
                    }
                }
            }

            return mailMessage;
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
