using System.Text.Json;

namespace Contracts;

public record MessageEnvelope(
    Guid MessageId,
    string MessageType,
    DateTime OccurredAtUtc,
    string JsonPayload
    )
{
    public static MessageEnvelope Create<T>(Guid messageId, DateTime nowUtc, T payload)
    {
        return new MessageEnvelope(
            MessageId: messageId,
            MessageType: typeof(T).Name,
            OccurredAtUtc: nowUtc,
            JsonPayload: JsonSerializer.Serialize(payload)
        );
    }

    public T ReadPayload<T>() => JsonSerializer.Deserialize<T>(JsonPayload)!;
}