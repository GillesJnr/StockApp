using MediatR;
using StockManagement.Application.Commands;
using StockManagement.Application.DTOs;
using StockManagement.Application.Services;

namespace StockManagement.Application.Handlers;

public class CreateControlledItemHandler : IRequestHandler<CreateControlledItemCommand, ControlledItemDto>
{
    private readonly ControlledItemService _service;

    public CreateControlledItemHandler(ControlledItemService service)
    {
        _service = service;
    }

    public Task<ControlledItemDto> Handle(CreateControlledItemCommand request, CancellationToken cancellationToken)
    {
        return _service.CreateAsync(request.Dto, request.InputterId, request.BranchId);
    }
}
