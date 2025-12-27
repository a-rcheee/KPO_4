using PaymentsService.Accounts;
using PaymentsService.Accounts.CreateAccount;
using PaymentsService.Accounts.GetBalance;
using PaymentsService.ProcessPaymentRequest;

namespace PaymentsService.UseCases;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<ICreateAccountHandler, CreateAccountHandler>();
        services.AddScoped<ITopUpHandler, TopUpHandler>();
        services.AddScoped<IGetBalanceHandler, GetBalanceHandler>();
        services.AddScoped<ProcessPaymentRequestHandler>();
        return services;
    }
}