namespace OrdersService.Messaging;

public interface IRabbitMqPublisher
{
    void Publish(string queue, object message, Guid messageId, CancellationToken ct);
}