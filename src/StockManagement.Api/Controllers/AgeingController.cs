using Microsoft.AspNetCore.Mvc;
using MediatR;
using StockManagement.Application.Queries;

namespace StockManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgeingController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgeingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate()
    {
        var res = await _mediator.Send(new EvaluateAgeingQuery());
        return Ok(res.Select(x => new {
            x.Item.Id,
            x.Item.ControlledNumber,
            x.DaysElapsed,
            x.DaysRemaining,
            x.PercentElapsed,
            Color = x.Color.ToString()
        }));
    }
}
