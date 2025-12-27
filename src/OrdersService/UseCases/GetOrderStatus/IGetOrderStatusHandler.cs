namespace OrdersService.UseCases.GetOrderStatus;

public interface IGetOrderStatusHandler
{
    Task<GetOrderStatusResponse?> Handle(string userId, Guid orderId, CancellationToken ct);
}