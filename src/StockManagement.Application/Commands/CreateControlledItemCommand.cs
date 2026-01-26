using MediatR;
using StockManagement.Application.DTOs;

namespace StockManagement.Application.Commands;

public record CreateControlledItemCommand(ControlledItemDto Dto, string InputterId, string BranchId) : IRequest<ControlledItemDto>;
