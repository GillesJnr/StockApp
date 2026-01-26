using Microsoft.EntityFrameworkCore;
using StockManagement.Infrastructure.Persistence;

namespace StockManagement.Tests.TestHelpers;

public static class InMemoryFactory
{
    public static StockDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new StockDbContext(options);
    }
}
