using MediatR;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Commands;

public record CreateTransactionCommand(ControlledTransaction Transaction) : IRequest<ControlledTransaction>;
