namespace PaymentsService.Entities;

public sealed class Account
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public long Balance { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}