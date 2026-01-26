using StockManagement.Domain.Entities;

namespace StockManagement.Domain.Interfaces;

public interface IReconciliationService
{
    Task PersistSnapshotAsync(string branchId, DateTime date);
}
