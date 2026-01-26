using System;

namespace StockManagement.Domain.Entities;

public class ReportSignOff
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReportRecordId { get; set; }
    public string AuthoriserId { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
    public string Comments { get; set; } = string.Empty;
}
