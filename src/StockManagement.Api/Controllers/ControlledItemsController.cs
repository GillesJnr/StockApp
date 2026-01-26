using Microsoft.AspNetCore.Mvc;
using MediatR;
using StockManagement.Application.Commands;
using StockManagement.Application.DTOs;
using StockManagement.Application.Queries;

namespace StockManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Inputter")]
public class ControlledItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ControlledItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ControlledItemDto dto, [FromQuery] string? branchId)
    {
        var inputterId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "user1";
        var b = branchId ?? "branchA";
        var created = await _mediator.Send(new CreateControlledItemCommand(dto, inputterId, b));
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _mediator.Send(new GetControlledItemsQuery());
        return Ok(list);
    }

    [HttpGet("reports/eod/pdf")]
    public async Task<IActionResult> GetEodPdf([FromQuery] string branchId = "branchA")
    {
        // Endpoint removed — use ReportsController endpoints for EOD report generation and retrieval.
        return NotFound();
    }
}
