using Contracts;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Infrastructure.Messaging;

public sealed class OutboxPublisherBackgroundService(IServiceProvider sp, TimeProvider time) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();
                var batch = await db.Outbox
                    .Where(x => x.PublishedAtUtc == null)
                    .OrderBy(x => x.OccurredAtUtc)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var msg in batch)
                {
                    try
                    {
                        var envelope = new MessageEnvelope(
                            MessageId: msg.Id,
                            MessageType: msg.Type,
                            OccurredAtUtc: msg.OccurredAtUtc,
                            JsonPayload: msg.PayloadJson
                        );

                        await publisher.PublishAsync(Queues.PaymentResults, envelope, msg.Id, stoppingToken);
                        msg.PublishedAtUtc = time.GetUtcNow().UtcDateTime;
                        msg.PublishAttempts += 1;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch
                    {
                        msg.PublishAttempts += 1;
                        await db.SaveChangesAsync(stoppingToken);
                        break;
                    }
                }
            }
            catch
            {
                // ignored
            }
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}