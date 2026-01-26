using StockManagement.Application.Commands;
using StockManagement.Application.Handlers;
using StockManagement.Domain.Entities;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests.Handlers;

public class SetOpeningBalanceHandlerTests
{
    [Fact]
    public async Task Handler_Sets_Opening_Balance()
    {
        var ctx = InMemoryFactory.CreateContext("handler_opening_db");
        var repo = new StockManagement.Infrastructure.Repositories.OpeningBalanceRepository(ctx);
        var handler = new SetOpeningBalanceHandler(repo);

        var ob = new OpeningBalance { BranchId = "branchA", Date = new DateTime(2024,8,19), Type = StockManagement.Domain.Enums.ControlledItemType.CreditCard, Quantity = 12 };

        var result = await handler.Handle(new SetOpeningBalanceCommand(ob), CancellationToken.None);

        Assert.Equal(12, result.Quantity);
        var stored = await repo.GetAsync("branchA", ob.Type, ob.Date);
        Assert.NotNull(stored);
        Assert.Equal(12, stored!.Quantity);
    }
}
