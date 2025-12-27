using OrdersService.Entities;

namespace OrdersService.UseCases.CreateOrder;

public sealed record CreateOrderResponse(Guid OrderId, OrderStatus Status);