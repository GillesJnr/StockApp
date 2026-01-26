using StockManagement.Application.Commands;
using StockManagement.Application.Handlers;
using StockManagement.Domain.Entities;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests.Handlers;

public class CreateTransactionHandlerTests
{
    [Fact]
    public async Task Handler_Adds_Transaction()
    {
        var ctx = InMemoryFactory.CreateContext("handler_tx_db");
        var repo = new StockManagement.Infrastructure.Repositories.ControlledTransactionRepository(ctx);
        var handler = new CreateTransactionHandler(repo);

        var tx = new ControlledTransaction { BranchId = "branchA", Date = DateTime.UtcNow.Date, Type = StockManagement.Domain.Enums.ControlledItemType.CreditCard, Kind = TransactionKind.Receipt, Quantity = 3 };

        var result = await handler.Handle(new CreateTransactionCommand(tx), CancellationToken.None);

        Assert.Equal(3, result.Quantity);
        var list = await repo.ListAsync(DateTime.UtcNow.Date, "branchA");
        Assert.Contains(list, t => t.Quantity == 3 && t.Kind == TransactionKind.Receipt);
    }
}
