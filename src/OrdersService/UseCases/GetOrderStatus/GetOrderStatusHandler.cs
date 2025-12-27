using Microsoft.EntityFrameworkCore;
using OrdersService.Infrastructure;

namespace OrdersService.UseCases.GetOrderStatus;

public sealed class GetOrderStatusHandler(OrdersDbContext db) : IGetOrderStatusHandler
{
    public async Task<GetOrderStatusResponse?> Handle(string userId, Guid orderId, CancellationToken ct)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Where(x => x.Id == orderId && x.UserId == userId)
            .Select(x => new GetOrderStatusResponse(x.Id, x.Amount, x.Status))
            .FirstOrDefaultAsync(ct);
        return order;
    }
}