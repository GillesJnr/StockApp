using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities;

namespace StockManagement.Infrastructure.Repositories;

public class ControlledTransactionRepository
{
    private readonly StockManagement.Infrastructure.Persistence.StockDbContext _db;

    public ControlledTransactionRepository(StockManagement.Infrastructure.Persistence.StockDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ControlledTransaction tx)
    {
        await _db.ControlledTransactions.AddAsync(tx);
        await _db.SaveChangesAsync();
    }

    public Task<List<ControlledTransaction>> ListAsync(DateTime? date = null, string? branchId = null)
    {
        var q = _db.ControlledTransactions.AsQueryable();
        if (date.HasValue) q = q.Where(x => x.Date.Date == date.Value.Date);
        if (!string.IsNullOrEmpty(branchId)) q = q.Where(x => x.BranchId == branchId);
        return q.ToListAsync();
    }
}
