using Contracts;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Entities;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.ProcessPaymentRequest;

public sealed class ProcessPaymentRequestHandler(PaymentsDbContext db, TimeProvider time)
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

        var req = envelope.ReadPayload<PaymentRequested>();

        var success = false;
        string? failReason = null;
        var rows = await db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE accounts
            SET balance = balance - {req.Amount}
            WHERE user_id = {req.UserId} AND balance >= {req.Amount};
        ", ct);

        if (rows == 1)
        {
            success = true;
        }
        else
        {
            success = false;
            failReason = "Insufficient funds or account not found";
        }

        db.Inbox.Add(new InboxMessage { MessageId = envelope.MessageId, ReceivedAtUtc = now });
        var result = new PaymentResult(req.OrderId, req.UserId, req.Amount, success, failReason);
        var outboxId = Guid.NewGuid();
        var outEnvelope = MessageEnvelope.Create(outboxId, now, result);

        db.Outbox.Add(new OutboxMessage
        {
            Id = outEnvelope.MessageId,
            Type = outEnvelope.MessageType,
            PayloadJson = outEnvelope.JsonPayload,
            OccurredAtUtc = outEnvelope.OccurredAtUtc,
            PublishedAtUtc = null,
            PublishAttempts = 0
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}