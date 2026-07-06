using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using ProcureHub.Modules.Notifications.Infrastructure.Settings;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.Modules.Notifications.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings              _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger   = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("Email not configured — skipping send to {To}: {Subject}", to, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port,
            _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);

        if (!string.IsNullOrEmpty(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
    }
}
