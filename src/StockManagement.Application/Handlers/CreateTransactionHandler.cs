using MediatR;
using StockManagement.Application.Commands;
using StockManagement.Domain.Entities;
using StockManagement.Infrastructure.Repositories;

namespace StockManagement.Application.Handlers;

public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, ControlledTransaction>
{
    private readonly ControlledTransactionRepository _repo;

    public CreateTransactionHandler(ControlledTransactionRepository repo)
    {
        _repo = repo;
    }

    public async Task<ControlledTransaction> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        await _repo.AddAsync(request.Transaction);
        return request.Transaction;
    }
}
