using OrdersService.Entities;

namespace OrdersService.UseCases.GetOrderStatus;

public sealed record GetOrderStatusResponse(Guid OrderId, long Amount, OrderStatus Status);