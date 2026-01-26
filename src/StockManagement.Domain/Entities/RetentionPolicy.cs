using System;
namespace StockManagement.Domain.Entities;

public class RetentionPolicy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public StockManagement.Domain.Enums.ControlledItemType Type { get; set; }
    public int RetentionDays { get; set; }
    public string? Notes { get; set; }
}
