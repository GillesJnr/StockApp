using Microsoft.AspNetCore.Mvc;
using MediatR;
using StockManagement.Domain.Entities;
using StockManagement.Application.Commands;
using StockManagement.Application.Queries;

namespace StockManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Inputter")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ControlledTransaction tx)
    {
        var created = await _mediator.Send(new CreateTransactionCommand(tx));
        return Ok(created);
    }

    [HttpPost("opening")]
    public async Task<IActionResult> SetOpening([FromBody] OpeningBalance ob)
    {
        var created = await _mediator.Send(new SetOpeningBalanceCommand(ob));
        return Ok(created);
    }
}
