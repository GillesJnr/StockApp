using StockManagement.Application.Handlers;
using StockManagement.Application.Queries;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests.Handlers;

public class EvaluateAgeingHandlerTests
{
    [Fact]
    public async Task Handler_Evaluates_Ageing_And_Notifies()
    {
        var ctx = InMemoryFactory.CreateContext("handler_ageing_db");
        var itemRepo = new StockManagement.Infrastructure.Repositories.ControlledItemRepository(ctx);
        var notifierLog = new List<(string to, string subject, string body)>();
        var notifier = new StockManagement.Tests.Handlers.EvalTestNotifier(notifierLog);

        var item = new ControlledItem
        {
            Type = ControlledItemType.CreditCard,
            DateReceived = DateTime.UtcNow.AddDays(-10),
            ReceivedFrom = "HO",
            AccountNumber = "123",
            ControlledNumber = "CC-AGE-1",
            Status = ControlledItemStatus.InBranch,
            ShortId = "S1",
            RetentionDays = 7,
            InputterId = "user1",
            BranchId = "branchA"
        };

        await itemRepo.AddAsync(item);
        var ageingService = new StockManagement.Application.Services.AgeingService(itemRepo, notifier);
        var handler = new EvaluateAgeingHandler(ageingService);

        var res = await handler.Handle(new EvaluateAgeingQuery(), CancellationToken.None);
        Assert.NotEmpty(res);
        Assert.True(notifierLog.Count >= 1);
    }

    public class EvalTestNotifier : StockManagement.Domain.Interfaces.INotificationPublisher
    {
        private readonly List<(string to, string subject, string body)> _log;
        public EvalTestNotifier(List<(string to, string subject, string body)> log) => _log = log;
        public Task PublishAsync(string to, string subject, string body)
        {
            _log.Add((to, subject, body));
            return Task.CompletedTask;
        }
    }
}
