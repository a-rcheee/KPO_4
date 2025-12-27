namespace OrdersService.Entities;

public sealed class Order
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public long Amount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}