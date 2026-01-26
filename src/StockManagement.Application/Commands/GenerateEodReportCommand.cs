using MediatR;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Commands;

public record GenerateEodReportCommand(string BranchId, DateTime Date, string GeneratedBy) : IRequest<ReportRecord>;
