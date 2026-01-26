using StockManagement.Application.Services;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Repositories;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests.Handlers;

public class CreateControlledItemCreatePathTests
{
    [Fact]
    public async Task Create_Sets_FraudFlag_And_Creates_Initial_Transaction_And_AccountName()
    {
        var ctx = InMemoryFactory.CreateContext("create_path_db");

        // seed a capture reason that triggers fraud flag
        await ctx.CaptureReasons.AddAsync(new CaptureReason { Code = "PLAIN_CARD", Description = "Plain card", ContactFraudOps = true, RaiseRiskEvent = false });
        await ctx.SaveChangesAsync();

        var itemRepo = new ControlledItemRepository(ctx);
        var txRepo = new ControlledTransactionRepository(ctx);
        var reasonRepo = new CaptureReasonRepository(ctx);

        // stub account lookup
        var accountLookup = new TestAccountLookup();

        var svc = new ControlledItemService(itemRepo, txRepo, reasonRepo, accountLookup);

        var dto = new StockManagement.Application.DTOs.ControlledItemDto
        {
            ControlledNumber = "T-CR-1",
            DateReceived = DateTime.UtcNow,
            RetentionDays = 30,
            Type = ControlledItemType.DebitCard,
            ReceivedFrom = "HO",
            AccountNumber = "AC123",
            ShortId = "S1",
            CaptureReasonCode = "PLAIN_CARD"
        };

        var res = await svc.CreateAsync(dto, "inputter1", "branchX");

        // verify persisted item
        var items = await itemRepo.ListAsync();
        var item = items.FirstOrDefault(i => i.ControlledNumber == "T-CR-1");
        Assert.NotNull(item);
        Assert.True(item!.FraudFlag);
        Assert.Equal("Account-AC123", item.AccountName);

        // verify transaction created
        var txs = await txRepo.ListAsync();
        Assert.Contains(txs, t => t.Reference == "T-CR-1" && t.Kind == TransactionKind.Receipt);
    }

    class TestAccountLookup : StockManagement.Application.Interfaces.IAccountLookup
    {
        public Task<string?> GetAccountNameAsync(string accountNumber) => Task.FromResult<string?>("Account-" + accountNumber);
    }
}
