using MediatR;
using StockManagement.Application.Services;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Queries;

public record CalculateEodQuery(string BranchId, DateTime Date) : IRequest<List<EodBalanceDto>>;
