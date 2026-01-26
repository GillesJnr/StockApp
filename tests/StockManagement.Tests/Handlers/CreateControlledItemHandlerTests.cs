using StockManagement.Application.Commands;
using StockManagement.Application.Handlers;
using StockManagement.Application.DTOs;
using StockManagement.Application.Services;
using StockManagement.Domain.Entities;
using StockManagement.Tests.TestHelpers;

namespace StockManagement.Tests.Handlers;

public class CreateControlledItemHandlerTests
{
    [Fact]
    public async Task Handler_Creates_Item()
    {
        var ctx = InMemoryFactory.CreateContext("handler_create_item_db");
        var repo = new StockManagement.Infrastructure.Repositories.ControlledItemRepository(ctx);
        var service = new ControlledItemService(repo);
        var handler = new CreateControlledItemHandler(service);

        var dto = new ControlledItemDto { ControlledNumber = "T-1", DateReceived = DateTime.UtcNow, RetentionDays = 10, Type = StockManagement.Domain.Enums.ControlledItemType.CreditCard, ReceivedFrom = "HO", AccountNumber = "1", AccountName = "A", ShortId = "S1" };

        var result = await handler.Handle(new CreateControlledItemCommand(dto, "user1", "branchA"), CancellationToken.None);

        Assert.NotNull(result);
        var list = await repo.ListAsync();
        Assert.Contains(list, i => i.ControlledNumber == "T-1");
    }
}
