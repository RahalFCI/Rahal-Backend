using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Templates
{
    public static class MailTemplates
    {
        public static MailRequest PasswordReset(string to, string userName, string otp) =>
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
                 Use the code below — this code expires in <strong>10 minutes</strong>.</p>
              <div style="background:#f3f4f6;padding:20px;border-radius:6px;text-align:center;margin:16px 0">
                <p style="font-size:32px;font-weight:bold;letter-spacing:4px;color:#4F46E5;margin:0">
                  {otp}
                </p>
              </div>
              <p style="color:#6B7280;font-size:13px">
                If you didn't request this, you can safely ignore this email.
              </p>
            </div>
            """
            };

        public static MailRequest VerificationOtp(string to, string userName, string otp) =>
            new()
            {
                To = to,
                DisplayName = userName,
                Subject = "Verify your email address",
                Body = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:auto">
              <h2>Verify your email address</h2>
              <p>Hi {userName},</p>
              <p>Thank you for registering with us! Use the code below to verify your email address — this code expires in <strong>10 minutes</strong>.</p>
              <div style="background:#f3f4f6;padding:20px;border-radius:6px;text-align:center;margin:16px 0">
                <p style="font-size:32px;font-weight:bold;letter-spacing:4px;color:#4F46E5;margin:0">
                  {otp}
                </p>
              </div>
              <p style="color:#6B7280;font-size:13px">
                If you didn't create this account, you can safely ignore this email.
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
