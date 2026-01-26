using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Repositories;
using StockManagement.Domain.Interfaces;

namespace StockManagement.Application.Services;

public enum AgeingColor
{
    Green,
    Brown,
    Yellow,
    LightRed,
    DeepRed
}

public class AgeingStatus
{
    public ControlledItem Item { get; set; } = null!;
    public int DaysElapsed { get; set; }
    public int DaysRemaining { get; set; }
    public double PercentElapsed { get; set; }
    public AgeingColor Color { get; set; }
}

public class AgeingService
{
    private readonly ControlledItemRepository _itemRepo;
    private readonly INotificationPublisher _notifier;

    public AgeingService(ControlledItemRepository itemRepo, INotificationPublisher notifier)
    {
        _itemRepo = itemRepo;
        _notifier = notifier;
    }

    public async Task<List<AgeingStatus>> EvaluateAsync()
    {
        var items = await _itemRepo.ListAsync();
        var list = new List<AgeingStatus>();
        foreach (var it in items)
        {
            var elapsed = (DateTime.UtcNow.Date - it.DateReceived.Date).Days;
            var daysRemaining = it.RetentionDays - elapsed;
            var percent = it.RetentionDays > 0 ? (double)elapsed / it.RetentionDays * 100.0 : 0.0;
            var color = GetColor(percent);

            var status = new AgeingStatus
            {
                Item = it,
                DaysElapsed = elapsed,
                DaysRemaining = daysRemaining,
                PercentElapsed = percent,
                Color = color
            };

            list.Add(status);

            if (percent >= 100.0 && it.Status == ControlledItemStatus.InBranch)
            {
                // Notify inputter and (placeholder) authoriser
                await _notifier.PublishAsync(it.InputterId, $"Retention period reached for {it.ControlledNumber}", $"Item {it.ControlledNumber} reached retention. Please take action.");
            }
        }

        return list;
    }

    private AgeingColor GetColor(double percent)
    {
        if (percent <= 70.0) return AgeingColor.Green;
        if (percent <= 90.0) return AgeingColor.Brown;
        if (percent < 100.0) return AgeingColor.Yellow;
        if (Math.Abs(percent - 100.0) < 0.0001) return AgeingColor.LightRed;
        return AgeingColor.DeepRed;
    }
}
