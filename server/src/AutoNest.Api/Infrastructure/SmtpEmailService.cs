using AutoNest.Business.Contracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AutoNest.Api.Infrastructure;

public sealed class SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendAsync(string recipient, string subject, string html, CancellationToken cancellationToken = default)
    {
        var host = config["Smtp:Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogInformation("SMTP is not configured. Suppressed email to {Recipient}: {Subject}", recipient, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(config["Smtp:From"] ?? "noreply@autonest.local"));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = html };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, config.GetValue("Smtp:Port", 587), SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);

        var user = config["Smtp:UserName"];

        if (!string.IsNullOrWhiteSpace(user))
        {
            await client.AuthenticateAsync(user, config["Smtp:Password"] ?? string.Empty, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
