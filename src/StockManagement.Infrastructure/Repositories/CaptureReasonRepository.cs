using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities;

namespace StockManagement.Infrastructure.Repositories;

public class CaptureReasonRepository
{
    private readonly StockManagement.Infrastructure.Persistence.StockDbContext _db;

    public CaptureReasonRepository(StockManagement.Infrastructure.Persistence.StockDbContext db)
    {
        _db = db;
    }

    public Task<CaptureReason?> GetByCodeAsync(string code) => _db.CaptureReasons.FirstOrDefaultAsync(x => x.Code == code);
    public Task<List<CaptureReason>> ListAsync() => _db.CaptureReasons.ToListAsync();
}
