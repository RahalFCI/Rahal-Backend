using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Templates
{
    public static class MailTemplates
    {
        public static MailRequest PasswordReset(string to, string userName, string resetLink) =>
            new()
            {
                To = to,
                DisplayName = userName,
                Subject = "Reset your password",
                Body = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:auto">
              <h2>Reset your password</h2>
              <p>Hi {userName},</p>
              <p>We received a request to reset your password.
                 Click the button below — this link expires in <strong>2 hours</strong>.</p>
              <a href="{resetLink}"
                 style="display:inline-block;padding:12px 24px;background:#4F46E5;
                        color:white;border-radius:6px;text-decoration:none;margin:16px 0">
                Reset password
              </a>
              <p style="color:#6B7280;font-size:13px">
                If you didn't request this, you can safely ignore this email.
              </p>
            </div>
            """
            };

        public static MailRequest Welcome(string to, string userName) =>
            new()
            {
                To = to,
                Subject = $"Welcome, {userName}!",
                Body = $"<h2>Welcome aboard, {userName}!</h2>"
            };
    }
}
