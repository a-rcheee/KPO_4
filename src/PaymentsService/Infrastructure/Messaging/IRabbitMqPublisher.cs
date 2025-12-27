namespace PaymentsService.Infrastructure.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync(string queue, object message, Guid messageId, CancellationToken ct);
}