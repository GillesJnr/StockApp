using StockManagement.Application.Commands;
using StockManagement.Application.Handlers;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests.Handlers;

public class GenerateAndSignOffReportHandlerTests
{
    [Fact]
    public async Task Generate_Handler_Creates_Report_And_SignOff_Handler_Records_Signoff()
    {
        var ctx = InMemoryFactory.CreateContext("handler_report_db");
        var txRepo = new StockManagement.Infrastructure.Repositories.ControlledTransactionRepository(ctx);
        var openingRepo = new StockManagement.Infrastructure.Repositories.OpeningBalanceRepository(ctx);
        var reportRepo = new StockManagement.Infrastructure.Repositories.ReportRepository(ctx);

        await openingRepo.SetAsync(new OpeningBalance { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Quantity = 5 });
        await txRepo.AddAsync(new ControlledTransaction { BranchId = "branchA", Type = ControlledItemType.CreditCard, Date = new DateTime(2024,8,19), Kind = TransactionKind.Receipt, Quantity = 2 });

        var recon = new StockManagement.Application.Services.ReconciliationService(txRepo, openingRepo);
        var reportService = new StockManagement.Application.Services.ReportService(recon, reportRepo);

        var genHandler = new GenerateEodReportHandler(reportService);
        var rec = await genHandler.Handle(new GenerateEodReportCommand("branchA", new DateTime(2024,8,19), "reconciler1"), CancellationToken.None);

        Assert.NotNull(rec);
        Assert.True(System.IO.File.Exists(rec.FilePath));

        var signHandler = new SignOffReportHandler(reportService);
        await signHandler.Handle(new SignOffReportCommand(rec.Id, "auth1", "OK"), CancellationToken.None);

        var sign = ctx.ReportSignOffs.FirstOrDefault(s => s.ReportRecordId == rec.Id);
        Assert.NotNull(sign);
        Assert.Equal("auth1", sign!.AuthoriserId);
    }
}
