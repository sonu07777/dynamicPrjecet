using System.Net;
using System.Net.Mail;

namespace BikeShowroomAPI.Services;

/// <summary>
/// SMTP email sender configured from the "Email" appsettings section.
/// When disabled (or when a send fails), the email body is logged to the console
/// so the password-reset flow remains usable in development without real SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (!_configuration.GetValue<bool>("Email:Enabled", true))
        {
            LogBodyToConsole(to, subject, htmlBody);
            return;
        }

        try
        {
            using var client = new SmtpClient
            {
                Host = _configuration["Email:Host"] ?? string.Empty,
                Port = _configuration.GetValue<int>("Email:Port", 587),
                EnableSsl = _configuration.GetValue<bool>("Email:EnableSsl", true),
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"])
            };

            var fromAddress = new MailAddress(
                _configuration["Email:From"] ?? "noreply@bikeshowroom.com",
                _configuration["Email:FromName"] ?? "BikeShowroom");

            var message = new MailMessage(fromAddress, new MailAddress(to))
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To} with subject '{Subject}'", to, subject);
        }
        catch (Exception ex)
        {
            // Never let an email failure break the calling flow.
            _logger.LogError(ex, "Failed to send email to {To}. See log body for fallback.", to);
            LogBodyToConsole(to, subject, htmlBody);
        }
    }

    private void LogBodyToConsole(string to, string subject, string htmlBody)
    {
        _logger.LogWarning(
            "[Email not sent - disabled or failed] To: {To} | Subject: {Subject} | Body: {Body}",
            to, subject, htmlBody);
    }
}
