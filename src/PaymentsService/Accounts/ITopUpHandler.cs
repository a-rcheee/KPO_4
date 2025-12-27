namespace PaymentsService.Accounts;

public interface ITopUpHandler
{
    Task<TopUpResponse> Handle(string userId, TopUpRequest request, CancellationToken ct);
}