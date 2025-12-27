namespace OrdersService.UseCases.ListOrders;

public interface IListOrdersHandler
{
    Task<ListOrdersResponse> Handle(string userId, CancellationToken ct);
}