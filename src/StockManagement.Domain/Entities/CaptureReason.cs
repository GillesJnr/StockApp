using System;

namespace StockManagement.Domain.Entities;

public class CaptureReason
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool ContactFraudOps { get; set; }
    public bool RaiseRiskEvent { get; set; }
}
