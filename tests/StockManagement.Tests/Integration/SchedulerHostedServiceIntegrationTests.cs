using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockManagement.Application.Services;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Persistence;
using StockManagement.Infrastructure.Repositories;
using StockManagement.Infrastructure.Services;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests.Integration;

public class SchedulerHostedServiceIntegrationTests
{
    [Fact]
    public async Task RunOnce_Persists_Snapshots_Generates_Report_And_Sends_Notifications()
    {
        var dbName = "scheduler_int_db" + Guid.NewGuid();
        var ctx = InMemoryFactory.CreateContext(dbName);

        // seed some data: one item and opening balance
        var item = new ControlledItem
        {
            Type = ControlledItemType.CreditCard,
            DateReceived = DateTime.UtcNow.AddDays(-1),
            ReceivedFrom = "HO",
            AccountNumber = "A1",
            ControlledNumber = "CC-INT-1",
            Status = ControlledItemStatus.InBranch,
            ShortId = "S1",
            RetentionDays = 10,
            InputterId = "user1",
            BranchId = "branchInt"
        };
        await ctx.ControlledItems.AddAsync(item);
        await ctx.OpeningBalances.AddAsync(new OpeningBalance { BranchId = "branchInt", Date = DateTime.UtcNow.Date, Type = ControlledItemType.CreditCard, Quantity = 5 });
        await ctx.SaveChangesAsync();

        // Build service provider
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> {
            ["Notifications:AuthoriserEmail"] = "authoriser@example.com"
        }).Build());
        services.AddLogging();
        services.AddSingleton(ctx);
        services.AddScoped<StockDbContext>(_ => ctx);
        services.AddScoped<ControlledItemRepository>();
        services.AddScoped<ControlledTransactionRepository>();
        services.AddScoped<OpeningBalanceRepository>();
        services.AddScoped<InventorySnapshotRepository>();
        services.AddScoped<CaptureReasonRepository>();
        services.AddScoped<ReportRepository>();
        services.AddScoped<ReconciliationService>();
        services.AddScoped<ReportService>();
        services.AddScoped<AgeingService>();
        services.AddScoped<ControlledItemService>();

        var sent = new List<(string to, string subject, string body)>();
        services.AddSingleton<StockManagement.Domain.Interfaces.INotificationPublisher>(new TestNotificationPublisher(sent));

        // account lookup stub
        services.AddSingleton<StockManagement.Application.Interfaces.IAccountLookup, TestAccountLookup>();

        var provider = services.BuildServiceProvider();

        // create scheduler
        var logger = provider.GetRequiredService<ILogger<SchedulerHostedService>>();
        var scheduler = new SchedulerHostedService(provider, logger, provider.GetRequiredService<IConfiguration>());

        // run once
        await scheduler.RunOnceAsync();

        // assert snapshots persisted
        var snaps = await ctx.InventorySnapshots.ToListAsync();
        Assert.NotEmpty(snaps);

        // assert report record saved
        var reports = await ctx.ReportRecords.Where(r => r.BranchId == "branchInt").ToListAsync();
        Assert.NotEmpty(reports);

        // assert notifications sent
        Assert.Contains(sent, s => s.to == "authoriser@example.com");
    }

    class TestNotificationPublisher : StockManagement.Domain.Interfaces.INotificationPublisher
    {
        private readonly List<(string to, string subject, string body)> _sent;
        public TestNotificationPublisher(List<(string to, string subject, string body)> sent) => _sent = sent;
        public Task PublishAsync(string to, string subject, string body)
        {
            _sent.Add((to, subject, body));
            return Task.CompletedTask;
        }
    }

    class TestAccountLookup : StockManagement.Application.Interfaces.IAccountLookup
    {
        public Task<string?> GetAccountNameAsync(string accountNumber) => Task.FromResult<string?>("TestAccount");
    }
}
