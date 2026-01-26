using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities;

namespace StockManagement.Infrastructure.Repositories;

public class InventorySnapshotRepository
{
    private readonly StockManagement.Infrastructure.Persistence.StockDbContext _db;

    public InventorySnapshotRepository(StockManagement.Infrastructure.Persistence.StockDbContext db)
    {
        _db = db;
    }

    public async Task SaveAsync(InventorySnapshot snapshot)
    {
        await _db.InventorySnapshots.AddAsync(snapshot);
        await _db.SaveChangesAsync();
    }

    public Task<List<InventorySnapshot>> ListAsync(string branchId, DateTime date) => _db.InventorySnapshots.Where(x => x.BranchId == branchId && x.Date.Date == date.Date).ToListAsync();
}
