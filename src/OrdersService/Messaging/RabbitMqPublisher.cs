using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrdersService.Messaging;

public sealed class RabbitMqPublisher(IConnection connection) : IRabbitMqPublisher
{
    public async void Publish(string queue, object message, Guid messageId, CancellationToken ct)
    {
        await using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct
        );
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties
        {
            Persistent = true,
            MessageId = messageId.ToString()
        };
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queue,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct
        );
    }
}