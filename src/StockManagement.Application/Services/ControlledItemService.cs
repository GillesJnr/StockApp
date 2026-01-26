using System.Globalization;
using StockManagement.Application.DTOs;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Repositories;

namespace StockManagement.Application.Services;

public class ControlledItemService
{
    private readonly ControlledItemRepository _repo;
    private readonly ControlledTransactionRepository _txRepo;
    private readonly CaptureReasonRepository _reasonRepo;
    private readonly StockManagement.Domain.Interfaces.IAccountLookup _accountLookup;

    public ControlledItemService(ControlledItemRepository repo, ControlledTransactionRepository txRepo, CaptureReasonRepository reasonRepo, StockManagement.Domain.Interfaces.IAccountLookup accountLookup)
    {
        _repo = repo;
        _txRepo = txRepo;
        _reasonRepo = reasonRepo;
        _accountLookup = accountLookup;
    }

    public async Task<ControlledItemDto> CreateAsync(ControlledItemDto dto, string inputterId, string branchId)
    {
        var accountName = dto.AccountName;
        if (string.IsNullOrWhiteSpace(accountName) && !string.IsNullOrWhiteSpace(dto.AccountNumber))
        {
            accountName = await _accountLookup.GetAccountNameAsync(dto.AccountNumber);
        }

        var entity = new ControlledItem
        {
            Type = dto.Type,
            DateReceived = dto.DateReceived,
            ReceivedFrom = dto.ReceivedFrom,
            AccountNumber = dto.AccountNumber,
            AccountName = accountName ?? string.Empty,
            ControlledNumber = dto.ControlledNumber,
            Status = ControlledItemStatus.InBranch,
            ShortId = dto.ShortId,
            GhanaCardNumber = dto.GhanaCardNumber,
            NameOnGhanaCard = dto.NameOnGhanaCard,
            RetentionDays = dto.RetentionDays,
            InputterId = inputterId,
            BranchId = branchId,
            PassportNumber = dto.PassportNumber,
            CaptureReason = dto.CaptureReasonCode
        };

        // Determine fraud flag if capture reason has flags
        if (!string.IsNullOrWhiteSpace(dto.CaptureReasonCode))
        {
            var reason = await _reasonRepo.GetByCodeAsync(dto.CaptureReasonCode);
            if (reason != null)
            {
                entity.FraudFlag = reason.ContactFraudOps || reason.RaiseRiskEvent;
            }
        }

        await _repo.AddAsync(entity);

        // Create initial receipt transaction for the new item
        var tx = new ControlledTransaction
        {
            BranchId = branchId,
            Date = DateTime.UtcNow,
            Kind = TransactionKind.Receipt,
            Quantity = 1,
            Type = entity.Type,
            Source = entity.ReceivedFrom,
            Reference = entity.ControlledNumber
        };

        await _txRepo.AddAsync(tx);

        dto.Id = entity.Id;
        dto.FraudFlag = entity.FraudFlag;
        return dto;
    }

    public Task<List<ControlledItemDto>> ListAsync()
    {
        return _repo.ListAsync().ContinueWith(t => t.Result.Select(x => new ControlledItemDto
        {
            Id = x.Id,
            Type = x.Type,
            DateReceived = x.DateReceived,
            ReceivedFrom = x.ReceivedFrom,
            AccountNumber = x.AccountNumber,
            AccountName = x.AccountName,
            ControlledNumber = x.ControlledNumber,
            Status = x.Status.ToString(),
            ShortId = x.ShortId,
            GhanaCardNumber = x.GhanaCardNumber,
            NameOnGhanaCard = x.NameOnGhanaCard,
            RetentionDays = x.RetentionDays
        }).ToList());
    }

    public async Task<byte[]> GenerateEodPdfAsync(string branchId)
    {
        var items = await _repo.ListAsync();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"EOD Report - {DateTime.UtcNow:yyyy-MM-dd}");
        sb.AppendLine($"Branch: {branchId}");
        foreach (var g in items.GroupBy(i => i.Type))
        {
            sb.AppendLine($"{g.Key}: {g.Count()} items");
        }

        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }
}
