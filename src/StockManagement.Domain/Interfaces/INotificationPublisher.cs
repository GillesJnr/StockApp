namespace StockManagement.Domain.Interfaces;

public interface INotificationPublisher
{
    Task PublishAsync(string to, string subject, string body);
}
