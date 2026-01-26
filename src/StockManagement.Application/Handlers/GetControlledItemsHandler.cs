using MediatR;
using StockManagement.Application.DTOs;
using StockManagement.Application.Queries;
using StockManagement.Application.Services;

namespace StockManagement.Application.Handlers;

public class GetControlledItemsHandler : IRequestHandler<GetControlledItemsQuery, List<ControlledItemDto>>
{
    private readonly ControlledItemService _service;

    public GetControlledItemsHandler(ControlledItemService service)
    {
        _service = service;
    }

    public Task<List<ControlledItemDto>> Handle(GetControlledItemsQuery request, CancellationToken cancellationToken)
    {
        return _service.ListAsync();
    }
}
