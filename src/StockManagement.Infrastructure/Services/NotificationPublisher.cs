using System.Text;
using StockManagement.Domain.Interfaces;

namespace StockManagement.Infrastructure.Services;

public class NotificationPublisher : INotificationPublisher
{
    private readonly string _outDir;

    public NotificationPublisher()
    {
        _outDir = Path.Combine(Directory.GetCurrentDirectory(), "data", "notifications");
        Directory.CreateDirectory(_outDir);
    }

    public Task PublishAsync(string to, string subject, string body)
    {
        var file = Path.Combine(_outDir, $"notify_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}.txt");
        var content = new StringBuilder();
        content.AppendLine($"To: {to}");
        content.AppendLine($"Subject: {subject}");
        content.AppendLine();
        content.AppendLine(body);
        File.WriteAllText(file, content.ToString());
        return Task.CompletedTask;
    }
}
