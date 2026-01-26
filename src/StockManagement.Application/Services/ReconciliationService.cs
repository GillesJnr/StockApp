using StockManagement.Application.DTOs;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Repositories;

namespace StockManagement.Application.Services;

public class EodBalanceDto
{
    public ControlledItemType Type { get; set; }
    public int Opening { get; set; }
    public int Receipts { get; set; }
    public int Issued { get; set; }
    public int Destroyed { get; set; }
    public int Closing { get; set; }
}

public class ReconciliationService
{
    private readonly ControlledTransactionRepository _txRepo;
    private readonly OpeningBalanceRepository _openingRepo;
        private readonly StockManagement.Infrastructure.Repositories.InventorySnapshotRepository? _snapshotRepo;

    public ReconciliationService(ControlledTransactionRepository txRepo, OpeningBalanceRepository openingRepo, StockManagement.Infrastructure.Repositories.InventorySnapshotRepository? snapshotRepo = null)
    {
        _txRepo = txRepo;
        _openingRepo = openingRepo;
        _snapshotRepo = snapshotRepo;
    }

    public async Task<List<EodBalanceDto>> CalculateEodAsync(string branchId, DateTime date)
    {
        var result = new List<EodBalanceDto>();
        foreach (ControlledItemType type in Enum.GetValues(typeof(ControlledItemType)))
        {
            var opening = await _openingRepo.GetAsync(branchId, type, date) ?? new OpeningBalance { Quantity = 0 };
            var txs = await _txRepo.ListAsync(date, branchId);
            var txForType = txs.Where(x => x.Type == type);
            var receipts = txForType.Where(x => x.Kind == TransactionKind.Receipt || x.Kind == TransactionKind.TransferIn).Sum(x => x.Quantity);
            var issued = txForType.Where(x => x.Kind == TransactionKind.Issue || x.Kind == TransactionKind.TransferOut).Sum(x => x.Quantity);
            var destroyed = txForType.Where(x => x.Kind == TransactionKind.Destroy).Sum(x => x.Quantity);

            var closing = opening.Quantity + receipts - issued - destroyed;

            result.Add(new EodBalanceDto
            {
                Type = type,
                Opening = opening.Quantity,
                Receipts = receipts,
                Issued = issued,
                Destroyed = destroyed,
                Closing = closing
            });
        }

        return result;
    }

    public async Task PersistSnapshotAsync(string branchId, DateTime date)
    {
        if (_snapshotRepo == null) return;

        var balances = await CalculateEodAsync(branchId, date);
        foreach (var b in balances)
        {
            var snap = new StockManagement.Domain.Entities.InventorySnapshot
            {
                BranchId = branchId,
                Date = date,
                Type = b.Type,
                Opening = b.Opening,
                Receipts = b.Receipts,
                Issued = b.Issued,
                Destroyed = b.Destroyed,
                Closing = b.Closing
            };
            await _snapshotRepo.SaveAsync(snap);
        }
    }
}
