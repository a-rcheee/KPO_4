using Microsoft.EntityFrameworkCore;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Accounts;

public sealed class TopUpHandler(PaymentsDbContext db) : ITopUpHandler
{
    public async Task<TopUpResponse> Handle(string userId, TopUpRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be > 0");
        }
        var rows = await db.Database.ExecuteSqlInterpolatedAsync($@" UPDATE accounts SET balance = balance + {request.Amount}
            WHERE user_id = {userId}; ", ct);

        if (rows == 0)
        {
            throw new InvalidOperationException("Account not found");
        }

        var balance = await db.Accounts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Balance)
            .FirstAsync(ct);

        return new TopUpResponse(userId, balance);
    }
}