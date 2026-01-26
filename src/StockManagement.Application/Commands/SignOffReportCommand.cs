using MediatR;

namespace StockManagement.Application.Commands;

public record SignOffReportCommand(System.Guid ReportId, string AuthoriserId, string Comments) : IRequest;
