using MediatR;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Commands;

public record SetOpeningBalanceCommand(OpeningBalance Opening) : IRequest<OpeningBalance>;
