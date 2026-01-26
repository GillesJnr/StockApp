using System;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Entities;

public enum TransactionKind
{
    Receipt,
    Issue,
    Destroy,
    TransferIn,
    TransferOut
}

public class ControlledTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ControlledItemType Type { get; set; }
    public string BranchId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionKind Kind { get; set; }
    public int Quantity { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}
