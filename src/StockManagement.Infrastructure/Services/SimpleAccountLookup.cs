using StockManagement.Domain.Interfaces;

namespace StockManagement.Infrastructure.Services;

public class SimpleAccountLookup : IAccountLookup
{
    public Task<string?> GetAccountNameAsync(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber)) return Task.FromResult<string?>(null);
        // Simple stub implementation. Replace with real integration.
        return Task.FromResult<string?>($"AccountName for {accountNumber}");
    }
}
