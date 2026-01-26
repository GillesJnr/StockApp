using MediatR;
using StockManagement.Application.Commands;
using StockManagement.Application.Services;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Handlers;

public class GenerateEodReportHandler : IRequestHandler<GenerateEodReportCommand, ReportRecord>
{
    private readonly ReportService _reportService;

    public GenerateEodReportHandler(ReportService reportService)
    {
        _reportService = reportService;
    }

    public Task<ReportRecord> Handle(GenerateEodReportCommand request, CancellationToken cancellationToken)
    {
        return _reportService.GenerateAndStoreEodAsync(request.BranchId, request.Date, request.GeneratedBy);
    }
}
