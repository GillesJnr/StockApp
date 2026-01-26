using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;

namespace StockManagement.Infrastructure.Repositories;

public class OpeningBalanceRepository
{
    private readonly StockManagement.Infrastructure.Persistence.StockDbContext _db;

    public OpeningBalanceRepository(StockManagement.Infrastructure.Persistence.StockDbContext db)
    {
        _db = db;
    }

    public async Task SetAsync(OpeningBalance ob)
    {
        var existing = await _db.OpeningBalances.FirstOrDefaultAsync(x => x.BranchId == ob.BranchId && x.Type == ob.Type && x.Date.Date == ob.Date.Date);
        if (existing == null)
        {
            await _db.OpeningBalances.AddAsync(ob);
        }
        else
        {
            existing.Quantity = ob.Quantity;
        }

        await _db.SaveChangesAsync();
    }

    public Task<OpeningBalance?> GetAsync(string branchId, ControlledItemType type, DateTime date)
        => _db.OpeningBalances.FirstOrDefaultAsync(x => x.BranchId == branchId && x.Type == type && x.Date.Date == date.Date);

    public Task<List<OpeningBalance>> ListAsync(string branchId, DateTime date)
        => _db.OpeningBalances.Where(x => x.BranchId == branchId && x.Date.Date == date.Date).ToListAsync();
}
