using OrdersService.Entities;

namespace OrdersService.UseCases.ListOrders;

public sealed record OrderDto(Guid Id, long Amount, OrderStatus Status, DateTime CreatedAtUtc);