namespace PaymentsService.Accounts;

public sealed record TopUpResponse(string UserId, long Balance);