using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Application.Templates;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Events;

namespace Users.Application.EventHandlers
{
    public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedEvent>
    {
        private readonly ILogger<SendWelcomeEmailHandler> _logger;
        private readonly IEmailService _emailService;

        public SendWelcomeEmailHandler(ILogger<SendWelcomeEmailHandler> logger, IEmailService emailservice)
        {
            _logger = logger;
            _emailService = emailservice;
        }

        public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending welcome email to user {UserId} with email {Email}", 
                    notification.UserId, notification.Email);

                // Create welcome email using the template
                var mailRequest = MailTemplates.Welcome(notification.Email, notification.Name);

                // Send the welcome email
                await _emailService.SendAsync(mailRequest, cancellationToken);

                _logger.LogInformation("Welcome email successfully sent to {Email}", notification.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending welcome email to {Email}. Error: {Error}", 
                    notification.Email, ex.Message);
            }
        }
    }
}
