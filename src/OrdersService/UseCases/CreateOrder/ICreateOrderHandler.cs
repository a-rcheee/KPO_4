namespace OrdersService.UseCases.CreateOrder;

public interface ICreateOrderHandler
{
    Task<CreateOrderResponse> Handle(string userId, CreateOrderRequest request, CancellationToken ct);
}