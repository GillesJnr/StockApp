using System.IO;
using StockManagement.Application.Services;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Repositories;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests;

public class ReportServiceTests
{
    [Fact]
    public async Task GenerateAndStoreEod_Should_Create_Pdf_And_Record()
    {
        var ctx = InMemoryFactory.CreateContext("report_db");
        var txRepo = new ControlledTransactionRepository(ctx);
        var openingRepo = new OpeningBalanceRepository(ctx);
        var reportRepo = new ReportRepository(ctx);

        // Seed simple data
        await openingRepo.SetAsync(new OpeningBalance { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Quantity = 5 });
        await txRepo.AddAsync(new ControlledTransaction { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Kind = TransactionKind.Receipt, Quantity = 2 });

        var recon = new ReconciliationService(txRepo, openingRepo);
        var service = new ReportService(recon, reportRepo);

        var rec = await service.GenerateAndStoreEodAsync("branchA", new DateTime(2024,8,19), "reconciler1");

        Assert.NotNull(rec);
        Assert.True(File.Exists(rec.FilePath));
    }
}
