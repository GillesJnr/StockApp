using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities;

namespace StockManagement.Infrastructure.Persistence;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options)
    {
    }

    public DbSet<ControlledItem> ControlledItems { get; set; } = null!;
    public DbSet<ControlledTransaction> ControlledTransactions { get; set; } = null!;
    public DbSet<OpeningBalance> OpeningBalances { get; set; } = null!;
    public DbSet<ReportRecord> ReportRecords { get; set; } = null!;
    public DbSet<ReportSignOff> ReportSignOffs { get; set; } = null!;
    public DbSet<RetentionPolicy> RetentionPolicies { get; set; } = null!;
    public DbSet<CaptureReason> CaptureReasons { get; set; } = null!;
    public DbSet<InventorySnapshot> InventorySnapshots { get; set; } = null!;
}
