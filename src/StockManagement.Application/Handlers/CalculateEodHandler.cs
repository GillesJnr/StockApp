using MediatR;
using StockManagement.Application.Queries;
using StockManagement.Application.Services;

namespace StockManagement.Application.Handlers;

public class CalculateEodHandler : IRequestHandler<CalculateEodQuery, List<EodBalanceDto>>
{
    private readonly ReconciliationService _recon;

    public CalculateEodHandler(ReconciliationService recon)
    {
        _recon = recon;
    }

    public Task<List<EodBalanceDto>> Handle(CalculateEodQuery request, CancellationToken cancellationToken)
    {
        return _recon.CalculateEodAsync(request.BranchId, request.Date);
    }
}
