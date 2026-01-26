using MediatR;
using StockManagement.Application.Services;

namespace StockManagement.Application.Queries;

public record EvaluateAgeingQuery() : IRequest<List<AgeingStatus>>;
