namespace PaymentsService.Accounts.GetBalance;

public interface IGetBalanceHandler
{
    Task<GetBalanceResponse?> Handle(string userId, CancellationToken ct);
}