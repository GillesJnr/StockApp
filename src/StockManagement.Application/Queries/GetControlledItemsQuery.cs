using MediatR;
using StockManagement.Application.DTOs;

namespace StockManagement.Application.Queries;

public record GetControlledItemsQuery() : IRequest<List<ControlledItemDto>>;
