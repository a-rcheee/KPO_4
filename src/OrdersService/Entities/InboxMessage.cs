namespace OrdersService.Entities;

public sealed class InboxMessage
{
    public Guid MessageId { get; set; } 
    public DateTime ReceivedAtUtc { get; set; }
}