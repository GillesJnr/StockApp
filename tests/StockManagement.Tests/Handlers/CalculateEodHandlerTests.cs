using StockManagement.Application.Handlers;
using StockManagement.Application.Queries;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests.Handlers;

public class CalculateEodHandlerTests
{
    [Fact]
    public async Task Handler_Computes_Eod()
    {
        var ctx = InMemoryFactory.CreateContext("handler_recon_db");
        var txRepo = new StockManagement.Infrastructure.Repositories.ControlledTransactionRepository(ctx);
        var openingRepo = new StockManagement.Infrastructure.Repositories.OpeningBalanceRepository(ctx);

        await openingRepo.SetAsync(new OpeningBalance { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Quantity = 10 });
        await txRepo.AddAsync(new ControlledTransaction { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Kind = TransactionKind.Receipt, Quantity = 4 });
        await txRepo.AddAsync(new ControlledTransaction { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Kind = TransactionKind.Issue, Quantity = 3 });
        await txRepo.AddAsync(new ControlledTransaction { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Kind = TransactionKind.Destroy, Quantity = 2 });

        var recon = new StockManagement.Application.Services.ReconciliationService(txRepo, openingRepo);
        var handler = new CalculateEodHandler(recon);

        var res = await handler.Handle(new CalculateEodQuery("branchA", new DateTime(2024,8,19)), CancellationToken.None);
        var credit = res.First(r => r.Type == ControlledItemType.CreditCard);
        Assert.Equal(9, credit.Closing);
    }
}
