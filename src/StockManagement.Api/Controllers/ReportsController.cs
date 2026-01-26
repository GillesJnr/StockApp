using Microsoft.AspNetCore.Mvc;
using StockManagement.Application.Services;

namespace StockManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly MediatR.IMediator _mediator;

    public ReportsController(MediatR.IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("eod/generate")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Reconciler")]
    public async Task<IActionResult> Generate([FromQuery] string branchId, [FromQuery] DateTime date, [FromQuery] string generatedBy)
    {
        var rec = await _mediator.Send(new StockManagement.Application.Commands.GenerateEodReportCommand(branchId, date, generatedBy));
        return Ok(new { rec.Id, rec.FilePath, rec.GeneratedAt });
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Authoriser,Reconciler")] 
    public async Task<IActionResult> List([FromQuery] string branchId)
    {
        var list = await _mediator.Send(new StockManagement.Application.Queries.GetReportsQuery(branchId));
        return Ok(list.Select(r => new { r.Id, r.BranchId, r.ReportDate, r.FilePath, r.GeneratedBy, r.GeneratedAt }));
    }

    public class SignOffRequest { public Guid ReportId { get; set; } public string AuthoriserId { get; set; } = string.Empty; public string Comments { get; set; } = string.Empty; }

    [HttpPost("eod/signoff")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Authoriser")]
    public async Task<IActionResult> SignOff([FromBody] SignOffRequest req)
    {
        await _mediator.Send(new StockManagement.Application.Commands.SignOffReportCommand(req.ReportId, req.AuthoriserId, req.Comments));
        return Ok(new { req.ReportId, req.AuthoriserId, SignedAt = DateTime.UtcNow });
    }
}
