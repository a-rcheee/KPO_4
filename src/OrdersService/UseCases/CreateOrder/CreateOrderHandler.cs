using Contracts;
using OrdersService.Entities;
using OrdersService.Infrastructure;

namespace OrdersService.UseCases.CreateOrder;

public sealed class CreateOrderHandler(OrdersDbContext db, TimeProvider time) : ICreateOrderHandler
{
    public async Task<CreateOrderResponse> Handle(string userId, CreateOrderRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be > 0");
        }

        var now = time.GetUtcNow().UtcDateTime;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = request.Amount,
            Status = OrderStatus.PendingPayment,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        db.Orders.Add(order);
        
        var msgId = Guid.NewGuid();
        var payload = new PaymentRequested(order.Id, userId, order.Amount);
        var envelope = MessageEnvelope.Create(msgId, now, payload);

        db.Outbox.Add(new OutboxMessage
        {
            Id = envelope.MessageId,
            Type = envelope.MessageType,
            PayloadJson = envelope.JsonPayload,
            OccurredAtUtc = envelope.OccurredAtUtc,
            PublishedAtUtc = null,
            PublishAttempts = 0
        });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new CreateOrderResponse(order.Id, order.Status);
    }
}