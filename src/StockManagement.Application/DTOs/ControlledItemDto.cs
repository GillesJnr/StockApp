using System;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.DTOs;

public class ControlledItemDto
{
    public Guid Id { get; set; }
    public ControlledItemType Type { get; set; }
    public DateTime DateReceived { get; set; }
    public string ReceivedFrom { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string ControlledNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ShortId { get; set; } = string.Empty;
    public string GhanaCardNumber { get; set; } = string.Empty;
    public string NameOnGhanaCard { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public string? CaptureReasonCode { get; set; }
    public string? PassportNumber { get; set; }
    public bool FraudFlag { get; set; }
}
