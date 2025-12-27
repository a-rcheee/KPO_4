namespace Contracts;

public sealed record PaymentResult(
    Guid OrderId,
    string UserId,
    long Amount,
    bool Success,
    string? FailReason
);