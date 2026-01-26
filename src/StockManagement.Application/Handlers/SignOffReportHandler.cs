using MediatR;
using StockManagement.Application.Commands;
using StockManagement.Application.Services;

namespace StockManagement.Application.Handlers;

public class SignOffReportHandler : IRequestHandler<SignOffReportCommand>
{
    private readonly ReportService _reportService;

    public SignOffReportHandler(ReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Unit> Handle(SignOffReportCommand request, CancellationToken cancellationToken)
    {
        await _reportService.AddSignOffAsync(request.ReportId, request.AuthoriserId, request.Comments);
        return Unit.Value;
    }
}
