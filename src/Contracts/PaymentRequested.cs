namespace Contracts;

public sealed record PaymentRequested(
    Guid OrderId,
    string UserId,
    long Amount
);