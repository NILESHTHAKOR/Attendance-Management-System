using AttendanceMS.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AttendanceMS.Services;

public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAttendanceWarningAsync(
        string toEmail, string studentName, decimal percent, string thresholdType)
    {
        try
        {
            var settings = _config.GetSection("EmailSettings");
            string subject, bodyHtml;

            if (thresholdType == "blacklisted")
            {
                subject  = "⚠️ BLACKLISTED – Attendance Critical";
                bodyHtml = BuildBlacklistEmail(studentName, percent);
            }
            else
            {
                subject  = "⚠️ Attendance Warning – Action Required";
                bodyHtml = BuildWarningEmail(studentName, percent);
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                settings["SenderName"] ?? "AttendanceMS",
                settings["SenderEmail"]!));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = bodyHtml };
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                settings["SmtpHost"]!,
                int.Parse(settings["SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(settings["Username"]!, settings["Password"]!);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email} for {Type}", toEmail, thresholdType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't rethrow — email failure should not break attendance marking
        }
    }

    private static string BuildWarningEmail(string name, decimal pct) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:Arial,sans-serif;background:#f5f5f5;margin:0;padding:20px;">
          <div style="max-width:560px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.1)">
            <div style="background:#f59e0b;padding:24px;text-align:center;">
              <h1 style="color:#fff;margin:0;font-size:22px;">⚠️ Attendance Warning</h1>
            </div>
            <div style="padding:28px;">
              <p>Dear <strong>{name}</strong>,</p>
              <p>Your attendance has dropped below the <strong>warning threshold</strong>.</p>
              <div style="background:#fffbeb;border:2px solid #f59e0b;border-radius:6px;padding:16px;margin:20px 0;text-align:center;">
                <p style="margin:0;font-size:32px;font-weight:bold;color:#b45309;">{pct:F1}%</p>
                <p style="margin:4px 0 0;color:#92400e;">Current Attendance</p>
              </div>
              <p>Please improve your attendance to avoid being <strong>blacklisted</strong>.</p>
              <p>Contact your class teacher if you need assistance.</p>
            </div>
            <div style="background:#f3f4f6;padding:14px;text-align:center;font-size:12px;color:#6b7280;">
              This is an automated notification from the Attendance Management System.
            </div>
          </div>
        </body>
        </html>
        """;

    private static string BuildBlacklistEmail(string name, decimal pct) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:Arial,sans-serif;background:#f5f5f5;margin:0;padding:20px;">
          <div style="max-width:560px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.1)">
            <div style="background:#dc2626;padding:24px;text-align:center;">
              <h1 style="color:#fff;margin:0;font-size:22px;">🚫 Attendance Blacklisted</h1>
            </div>
            <div style="padding:28px;">
              <p>Dear <strong>{name}</strong>,</p>
              <p>Your attendance has fallen critically low. You have been <strong>BLACKLISTED</strong>.</p>
              <div style="background:#fef2f2;border:2px solid #dc2626;border-radius:6px;padding:16px;margin:20px 0;text-align:center;">
                <p style="margin:0;font-size:32px;font-weight:bold;color:#dc2626;">{pct:F1}%</p>
                <p style="margin:4px 0 0;color:#991b1b;">Current Attendance</p>
              </div>
              <p>Being blacklisted may affect your eligibility for exams. Please meet with your Head of Department <strong>immediately</strong>.</p>
            </div>
            <div style="background:#f3f4f6;padding:14px;text-align:center;font-size:12px;color:#6b7280;">
              This is an automated notification from the Attendance Management System.
            </div>
          </div>
        </body>
        </html>
        """;
}
