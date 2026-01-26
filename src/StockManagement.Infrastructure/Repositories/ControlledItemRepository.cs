using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities;

namespace StockManagement.Infrastructure.Repositories;

public class ControlledItemRepository
{
    private readonly StockManagement.Infrastructure.Persistence.StockDbContext _db;

    public ControlledItemRepository(StockManagement.Infrastructure.Persistence.StockDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ControlledItem item)
    {
        await _db.ControlledItems.AddAsync(item);
        await _db.SaveChangesAsync();
    }

    public Task<List<ControlledItem>> ListAsync() => _db.ControlledItems.ToListAsync();

    public Task<ControlledItem?> GetAsync(Guid id) => _db.ControlledItems.FirstOrDefaultAsync(x => x.Id == id);
}
