using MediatR;
using StockManagement.Application.Queries;
using StockManagement.Domain.Entities;
using StockManagement.Infrastructure.Repositories;

namespace StockManagement.Application.Handlers;

public class GetReportsHandler : IRequestHandler<GetReportsQuery, List<ReportRecord>>
{
    private readonly ReportRepository _repo;

    public GetReportsHandler(ReportRepository repo)
    {
        _repo = repo;
    }

    public Task<List<ReportRecord>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        return _repo.ListAsync(request.BranchId);
    }
}
