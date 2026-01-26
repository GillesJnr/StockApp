using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using StockManagement.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace StockManagement.Infrastructure.Services;

public class SchedulerHostedService : IHostedService, IDisposable
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SchedulerHostedService> _logger;
    private System.Threading.Timer? _timer;
    private readonly TimeSpan _dailyTime;

    public SchedulerHostedService(IServiceProvider services, ILogger<SchedulerHostedService> logger, IConfiguration config)
    {
        _services = services;
        _logger = logger;
        // read scheduled time from config (HH:mm) or default to 23:59
        var t = config.GetValue<string>("Scheduler:ReconciliationTime") ?? "23:59";
        if (TimeSpan.TryParse(t, out var ts)) _dailyTime = ts; else _dailyTime = new TimeSpan(23,59,0);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SchedulerHostedService starting.");
        var next = GetNextRun(_dailyTime);
        var delay = next - DateTime.UtcNow;
        if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
        _timer = new System.Threading.Timer(async _ => await DoWork(), null, delay, Timeout.InfiniteTimeSpan);
        return Task.CompletedTask;
    }

    private DateTime GetNextRun(TimeSpan dailyTime)
    {
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day, dailyTime.Hours, dailyTime.Minutes, 0, DateTimeKind.Utc);
        if (today > now) return today;
        return today.AddDays(1);
    }

    private async Task DoWork()
    {
        try
        {
            using var scope = _services.CreateScope();
            // Resolve application services by runtime type to avoid compile-time project reference
            var ageingType = Type.GetType("StockManagement.Application.Services.AgeingService, StockManagement.Application");
            var reconType = Type.GetType("StockManagement.Application.Services.ReconciliationService, StockManagement.Application");
            var reportType = Type.GetType("StockManagement.Application.Services.ReportService, StockManagement.Application");
            if (ageingType == null || reconType == null || reportType == null)
            {
                _logger.LogWarning("Could not resolve application service types for scheduler run.");
                return;
            }

            var ageing = scope.ServiceProvider.GetRequiredService(ageingType);
            var recon = scope.ServiceProvider.GetRequiredService(reconType);
            var report = scope.ServiceProvider.GetRequiredService(reportType);
            var notifier = scope.ServiceProvider.GetRequiredService<StockManagement.Domain.Interfaces.INotificationPublisher>();
            var db = scope.ServiceProvider.GetRequiredService<StockManagement.Infrastructure.Persistence.StockDbContext>();

            _logger.LogInformation("Scheduler running ageing evaluation...");
            // invoke EvaluateAsync via reflection
            var evalMethod = ageingType.GetMethod("EvaluateAsync");
            if (evalMethod != null)
            {
                var evalTask = (Task?)evalMethod.Invoke(ageing, null);
                if (evalTask != null) await evalTask;
            }

            // Compute branches from data
            var branches = await db.ControlledItems.Select(x => x.BranchId).Distinct().ToListAsync();
            if (!branches.Any()) branches = await db.OpeningBalances.Select(x => x.BranchId).Distinct().ToListAsync();

            var date = DateTime.UtcNow.Date;
            foreach (var branch in branches)
            {
                _logger.LogInformation("Running reconciliation for branch {branch}", branch);
                // invoke PersistSnapshotAsync on reconciliation service
                var persistMethod = reconType.GetMethod("PersistSnapshotAsync");
                if (persistMethod != null)
                {
                    var persistTask = (Task?)persistMethod.Invoke(recon, new object?[] { branch, date });
                    if (persistTask != null) await persistTask;
                }

                // invoke GenerateAndStoreEodAsync on report service
                var genMethod = reportType.GetMethod("GenerateAndStoreEodAsync");
                object? rec = null;
                if (genMethod != null)
                {
                    var genTaskObj = genMethod.Invoke(report, new object?[] { branch, date, "system" });
                    if (genTaskObj is Task genTask)
                    {
                        await genTask;
                        var resultProp = genTask.GetType().GetProperty("Result");
                        if (resultProp != null) rec = resultProp.GetValue(genTask);
                    }
                }
                // notify authoriser if configured
                var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var authoriser = cfg.GetValue<string>("Notifications:AuthoriserEmail");
                if (!string.IsNullOrWhiteSpace(authoriser))
                {
                    var filePath = rec?.GetType().GetProperty("FilePath")?.GetValue(rec)?.ToString() ?? "";
                    await notifier.PublishAsync(authoriser, $"EOD Report for {branch} - {date:yyyy-MM-dd}", $"Report generated: {filePath}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduler work failed");
        }
        finally
        {
            // schedule next run
            var next = GetNextRun(_dailyTime);
            var delay = next - DateTime.UtcNow;
            _timer?.Change(delay, Timeout.InfiniteTimeSpan);
        }
    }

    // Exposed for testing to run the scheduled work once without affecting timer
    public async Task RunOnceAsync()
    {
        try
        {
            using var scope = _services.CreateScope();
            // Resolve application services by runtime type to avoid compile-time project reference
            var ageingType = Type.GetType("StockManagement.Application.Services.AgeingService, StockManagement.Application");
            var reconType = Type.GetType("StockManagement.Application.Services.ReconciliationService, StockManagement.Application");
            var reportType = Type.GetType("StockManagement.Application.Services.ReportService, StockManagement.Application");
            if (ageingType == null || reconType == null || reportType == null)
            {
                _logger.LogWarning("Could not resolve application service types for scheduler run.");
                return;
            }

            var ageing = scope.ServiceProvider.GetRequiredService(ageingType);
            var recon = scope.ServiceProvider.GetRequiredService(reconType);
            var report = scope.ServiceProvider.GetRequiredService(reportType);
            var notifier = scope.ServiceProvider.GetRequiredService<StockManagement.Domain.Interfaces.INotificationPublisher>();
            var db = scope.ServiceProvider.GetRequiredService<StockManagement.Infrastructure.Persistence.StockDbContext>();

            _logger.LogInformation("Scheduler RunOnce running ageing evaluation...");
            // invoke EvaluateAsync via reflection
            var evalMethod = ageingType.GetMethod("EvaluateAsync");
            if (evalMethod != null)
            {
                var evalTask = (Task?)evalMethod.Invoke(ageing, null);
                if (evalTask != null) await evalTask;
            }

            // Compute branches from data
            var branches = await db.ControlledItems.Select(x => x.BranchId).Distinct().ToListAsync();
            if (!branches.Any()) branches = await db.OpeningBalances.Select(x => x.BranchId).Distinct().ToListAsync();

            var date = DateTime.UtcNow.Date;
            foreach (var branch in branches)
            {
                _logger.LogInformation("Running reconciliation for branch {branch}", branch);
                // invoke PersistSnapshotAsync on reconciliation service
                var persistMethod = reconType.GetMethod("PersistSnapshotAsync");
                if (persistMethod != null)
                {
                    var persistTask = (Task?)persistMethod.Invoke(recon, new object?[] { branch, date });
                    if (persistTask != null) await persistTask;
                }

                // invoke GenerateAndStoreEodAsync on report service
                var genMethod = reportType.GetMethod("GenerateAndStoreEodAsync");
                object? rec = null;
                if (genMethod != null)
                {
                    var genTaskObj = genMethod.Invoke(report, new object?[] { branch, date, "system" });
                    if (genTaskObj is Task genTask)
                    {
                        await genTask;
                        var resultProp = genTask.GetType().GetProperty("Result");
                        if (resultProp != null) rec = resultProp.GetValue(genTask);
                    }
                }

                // notify authoriser if configured
                var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var authoriser = cfg.GetValue<string>("Notifications:AuthoriserEmail");
                if (!string.IsNullOrWhiteSpace(authoriser))
                {
                    var filePath = rec?.GetType().GetProperty("FilePath")?.GetValue(rec)?.ToString() ?? "";
                    await notifier.PublishAsync(authoriser, $"EOD Report for {branch} - {date:yyyy-MM-dd}", $"Report generated: {filePath}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduler RunOnce failed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SchedulerHostedService stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
