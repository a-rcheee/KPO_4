using Microsoft.AspNetCore.Mvc;
using PaymentsService.Accounts;
using PaymentsService.Accounts.CreateAccount;
using PaymentsService.Accounts.GetBalance;

namespace PaymentsService.Presentation;

public static class AccountsEndpoints
{
    public static WebApplication MapAccountsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/accounts").WithTags("Payments");

        group.MapPost("",
                async ([FromHeader(Name = "User-Id")] string? userId,
                    ICreateAccountHandler handler,
                    CancellationToken ct) =>
                {
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        return Results.BadRequest("Missing User-Id header");
                    }

                    var response = await handler.Handle(userId, ct);
                    return Results.Ok(response);
                })
            .WithName("CreateAccount")
            .WithSummary("Create account")
            .WithOpenApi();

        group.MapPost("topup",
                async ([FromHeader(Name = "User-Id")] string? userId,
                    TopUpRequest request,
                    ITopUpHandler handler,
                    CancellationToken ct) =>
                {
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        return Results.BadRequest("Missing User-Id header");
                    }

                    var response = await handler.Handle(userId, request, ct);
                    return Results.Ok(response);
                })
            .WithName("TopUp")
            .WithSummary("Top up account balance")
            .WithOpenApi();

        group.MapGet("balance",
                async ([FromHeader(Name = "User-Id")] string? userId,
                    IGetBalanceHandler handler,
                    CancellationToken ct) =>
                {
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        return Results.BadRequest("Missing User-Id header");
                    }

                    var response = await handler.Handle(userId, ct);
                    return Results.Ok(response);
                })
            .WithName("GetBalance")
            .WithSummary("Get current balance")
            .WithOpenApi();

        return app;
    }
}