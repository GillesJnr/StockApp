using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities;

namespace StockManagement.Infrastructure.Repositories;

public class ReportRepository
{
    private readonly StockManagement.Infrastructure.Persistence.StockDbContext _db;
    private readonly string _storageDir;

    public ReportRepository(StockManagement.Infrastructure.Persistence.StockDbContext db)
    {
        _db = db;
        _storageDir = Path.Combine(Directory.GetCurrentDirectory(), "data", "reports");
        Directory.CreateDirectory(_storageDir);
    }

    public async Task<ReportRecord> SaveReportAsync(string branchId, DateTime reportDate, string generatedBy, byte[] pdfBytes)
    {
        var fileName = $"EOD_{branchId}_{reportDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var path = Path.Combine(_storageDir, fileName);
        await File.WriteAllBytesAsync(path, pdfBytes);

        var rec = new ReportRecord
        {
            BranchId = branchId,
            ReportDate = reportDate,
            FilePath = path,
            GeneratedBy = generatedBy,
            GeneratedAt = DateTime.UtcNow
        };

        await _db.ReportRecords.AddAsync(rec);
        await _db.SaveChangesAsync();
        return rec;
    }

    public Task<ReportRecord?> GetAsync(Guid id) => _db.ReportRecords.FirstOrDefaultAsync(x => x.Id == id);

    public Task<List<ReportRecord>> ListAsync(string branchId) => _db.ReportRecords.Where(x => x.BranchId == branchId).ToListAsync();

    public async Task AddSignOffAsync(ReportSignOff signOff)
    {
        await _db.ReportSignOffs.AddAsync(signOff);
        await _db.SaveChangesAsync();
    }
}
