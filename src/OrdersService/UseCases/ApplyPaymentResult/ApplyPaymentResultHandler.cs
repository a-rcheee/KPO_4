using Contracts;
using Microsoft.EntityFrameworkCore;
using OrdersService.Entities;
using OrdersService.Infrastructure;

namespace OrdersService.UseCases.ApplyPaymentResult;

public sealed class ApplyPaymentResultHandler(OrdersDbContext db, TimeProvider time)
{
    public async Task Handle(MessageEnvelope envelope, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var already = await db.Inbox.AnyAsync(x => x.MessageId == envelope.MessageId, ct);
        if (already)
        {
            await tx.CommitAsync(ct);
            return;
        }
        var result = envelope.ReadPayload<PaymentResult>();
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == result.OrderId, ct);
        if (order is not null)
        {
            if (order.Status == OrderStatus.PendingPayment)
            {
                order.Status = result.Success ? OrderStatus.Paid : OrderStatus.Failed;
                order.UpdatedAtUtc = now;
            }
        }
        db.Inbox.Add(new InboxMessage { MessageId = envelope.MessageId, ReceivedAtUtc = now });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}