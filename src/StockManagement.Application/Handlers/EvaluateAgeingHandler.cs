using MediatR;
using StockManagement.Application.Queries;
using StockManagement.Application.Services;

namespace StockManagement.Application.Handlers;

public class EvaluateAgeingHandler : IRequestHandler<EvaluateAgeingQuery, List<AgeingStatus>>
{
    private readonly AgeingService _ageingService;

    public EvaluateAgeingHandler(AgeingService ageingService)
    {
        _ageingService = ageingService;
    }

    public Task<List<AgeingStatus>> Handle(EvaluateAgeingQuery request, CancellationToken cancellationToken)
    {
        return _ageingService.EvaluateAsync();
    }
}
