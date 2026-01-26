using MediatR;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Queries;

public record GetReportsQuery(string BranchId) : IRequest<List<ReportRecord>>;
