namespace StockManagement.Domain.Interfaces;

public interface IAccountLookup
{
    Task<string?> GetAccountNameAsync(string accountNumber);
}
