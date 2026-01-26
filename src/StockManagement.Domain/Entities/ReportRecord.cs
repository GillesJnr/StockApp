using System;

namespace StockManagement.Domain.Entities;

public class ReportRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BranchId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string GeneratedBy { get; set; } = string.Empty; // reconciler
    public DateTime GeneratedAt { get; set; }
}
