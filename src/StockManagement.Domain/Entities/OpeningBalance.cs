using System;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Entities;

public class OpeningBalance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BranchId { get; set; } = string.Empty;
    public ControlledItemType Type { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}
