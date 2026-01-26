using MediatR;
using StockManagement.Application.Commands;
using StockManagement.Domain.Entities;
using StockManagement.Infrastructure.Repositories;

namespace StockManagement.Application.Handlers;

public class SetOpeningBalanceHandler : IRequestHandler<SetOpeningBalanceCommand, OpeningBalance>
{
    private readonly OpeningBalanceRepository _repo;

    public SetOpeningBalanceHandler(OpeningBalanceRepository repo)
    {
        _repo = repo;
    }

    public async Task<OpeningBalance> Handle(SetOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        await _repo.SetAsync(request.Opening);
        return request.Opening;
    }
}
