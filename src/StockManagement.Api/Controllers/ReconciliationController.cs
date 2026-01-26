using Microsoft.AspNetCore.Mvc;
using StockManagement.Application.Services;

namespace StockManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Inputter,Authoriser")]
public class ReconciliationController : ControllerBase
{
    private readonly MediatR.IMediator _mediator;

    public ReconciliationController(MediatR.IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("eod")]
    public async Task<IActionResult> GetEod([FromQuery] string branchId, [FromQuery] DateTime date)
    {
        var balances = await _mediator.Send(new StockManagement.Application.Queries.CalculateEodQuery(branchId, date));
        return Ok(balances);
    }
}
