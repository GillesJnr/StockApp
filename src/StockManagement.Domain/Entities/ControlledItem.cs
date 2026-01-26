using System;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Entities;

public class ControlledItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ControlledItemType Type { get; set; }
    public DateTime DateReceived { get; set; }
    public string ReceivedFrom { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string ControlledNumber { get; set; } = string.Empty;
    public ControlledItemStatus Status { get; set; }
    public string ShortId { get; set; } = string.Empty;
    public string GhanaCardNumber { get; set; } = string.Empty;
    public string NameOnGhanaCard { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public string? CaptureReason { get; set; }
    public string? PassportNumber { get; set; }
    public bool FraudFlag { get; set; }
    public string InputterId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
}
