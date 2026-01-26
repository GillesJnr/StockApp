using System;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Entities;

public class InventorySnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BranchId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public ControlledItemType Type { get; set; }
    public int Opening { get; set; }
    public int Receipts { get; set; }
    public int Issued { get; set; }
    public int Destroyed { get; set; }
    public int Closing { get; set; }
}
