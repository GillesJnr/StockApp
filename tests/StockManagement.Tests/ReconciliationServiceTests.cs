using StockManagement.Application.Services;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Repositories;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests;

public class ReconciliationServiceTests
{
    [Fact]
    public async Task CalculateEod_Should_Return_Correct_Closing()
    {
        var ctx = InMemoryFactory.CreateContext("recon_db");
        var txRepo = new ControlledTransactionRepository(ctx);
        var openingRepo = new OpeningBalanceRepository(ctx);

        // Opening 10 credit cards
        await openingRepo.SetAsync(new OpeningBalance { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Quantity = 10 });

        // Receipts +4, issues -3, destroys -2
        await txRepo.AddAsync(new ControlledTransaction { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Kind = TransactionKind.Receipt, Quantity = 4 });
        await txRepo.AddAsync(new ControlledTransaction { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Kind = TransactionKind.Issue, Quantity = 3 });
        await txRepo.AddAsync(new ControlledTransaction { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Kind = TransactionKind.Destroy, Quantity = 2 });

        var recon = new ReconciliationService(txRepo, openingRepo);
        var balances = await recon.CalculateEodAsync("branchA", new DateTime(2024,8,19));

        var credit = balances.First(b => b.Type == ControlledItemType.CreditCard);
        Assert.Equal(10, credit.Opening);
        Assert.Equal(4, credit.Receipts);
        Assert.Equal(3, credit.Issued);
        Assert.Equal(2, credit.Destroyed);
        Assert.Equal(9, credit.Closing);
    }
}
