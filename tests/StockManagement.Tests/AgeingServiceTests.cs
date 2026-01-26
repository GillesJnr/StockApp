using StockManagement.Application.Services;
using StockManagement.Domain.Interfaces;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Repositories;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests;

public class AgeingServiceTests
{
    [Fact]
    public async Task Evaluate_Should_Set_Color_And_Send_Notification_When_Expired()
    {
        var ctx = InMemoryFactory.CreateContext("ageing_db");
        var repo = new ControlledItemRepository(ctx);

        var item = new ControlledItem
        {
            Type = ControlledItemType.CreditCard,
            DateReceived = DateTime.UtcNow.AddDays(-10),
            ReceivedFrom = "ho",
            AccountNumber = "123",
            ControlledNumber = "CC-001",
            Status = ControlledItemStatus.InBranch,
            ShortId = "S1",
            GhanaCardNumber = "",
            NameOnGhanaCard = "",
            RetentionDays = 7,
            InputterId = "user1",
            BranchId = "branchA"
        };

        await repo.AddAsync(item);

        var sent = new List<(string to, string subject, string body)>();
        var notifier = new TestNotificationPublisher(sent);

        var svc = new AgeingService(repo, notifier);
        var res = await svc.EvaluateAsync();

        var status = res.FirstOrDefault(r => r.Item.ControlledNumber == "CC-001");
        Assert.NotNull(status);
        Assert.Equal(AgeingColor.DeepRed, status!.Color);
        Assert.True(sent.Count >= 1);
    }

    class TestNotificationPublisher : INotificationPublisher
    {
        private readonly List<(string to, string subject, string body)> _sent;
        public TestNotificationPublisher(List<(string to, string subject, string body)> sent) => _sent = sent;
        public Task PublishAsync(string to, string subject, string body)
        {
            _sent.Add((to, subject, body));
            return Task.CompletedTask;
        }
    }
}
