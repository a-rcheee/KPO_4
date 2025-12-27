namespace OrdersService.UseCases.ListOrders;

public sealed record ListOrdersResponse(IReadOnlyList<OrderDto> Orders);