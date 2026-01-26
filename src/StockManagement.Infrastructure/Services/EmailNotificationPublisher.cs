using MailKit.Net.Smtp;
using MimeKit;
using StockManagement.Domain.Interfaces;

namespace StockManagement.Infrastructure.Services;

public class EmailNotificationPublisher : INotificationPublisher
{
    private readonly string _fallbackDir;

    public EmailNotificationPublisher()
    {
        _fallbackDir = Path.Combine(Directory.GetCurrentDirectory(), "data", "notifications");
        Directory.CreateDirectory(_fallbackDir);
    }

    public async Task PublishAsync(string to, string subject, string body)
    {
        var host = Environment.GetEnvironmentVariable("SMTP_HOST");
        if (string.IsNullOrEmpty(host))
        {
            // fallback to file-based notification
            var file = Path.Combine(_fallbackDir, $"notify_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}.txt");
            await File.WriteAllTextAsync(file, $"To: {to}\nSubject: {subject}\n\n{body}");
            return;
        }

        var port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var p) ? p : 25;
        var user = Environment.GetEnvironmentVariable("SMTP_USER");
        var pass = Environment.GetEnvironmentVariable("SMTP_PASS");
        var from = Environment.GetEnvironmentVariable("SMTP_FROM") ?? user ?? "no-reply@example.com";

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.Auto);
        if (!string.IsNullOrEmpty(user)) await client.AuthenticateAsync(user, pass ?? string.Empty);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
