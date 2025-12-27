namespace PaymentsService.Accounts.CreateAccount;

public interface ICreateAccountHandler
{
    Task<CreateAccountResponse> Handle(string userId, CancellationToken ct);
}