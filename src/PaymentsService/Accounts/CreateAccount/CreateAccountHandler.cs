using Microsoft.EntityFrameworkCore;
using PaymentsService.Entities;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Accounts.CreateAccount;

public sealed class CreateAccountHandler(PaymentsDbContext db, TimeProvider time) : ICreateAccountHandler
{
    public async Task<CreateAccountResponse> Handle(string userId, CancellationToken ct)
    {
        var exists = await db.Accounts.AnyAsync(x => x.UserId == userId, ct);
        if (exists)
        {
            throw new InvalidOperationException("Account already exists for this user");
        }

        var now = time.GetUtcNow().UtcDateTime;
        db.Accounts.Add(new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 0,
            CreatedAtUtc = now
        });

        await db.SaveChangesAsync(ct);
        return new CreateAccountResponse(userId, 0);
    }
}