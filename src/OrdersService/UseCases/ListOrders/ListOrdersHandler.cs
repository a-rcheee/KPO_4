using Microsoft.EntityFrameworkCore;
using OrdersService.Infrastructure;

namespace OrdersService.UseCases.ListOrders;

public sealed class ListOrdersHandler(OrdersDbContext db) : IListOrdersHandler
{
    public async Task<ListOrdersResponse> Handle(string userId, CancellationToken ct)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new OrderDto(x.Id, x.Amount, x.Status, x.CreatedAtUtc))
            .ToListAsync(ct);
        return new ListOrdersResponse(orders);
    }
}