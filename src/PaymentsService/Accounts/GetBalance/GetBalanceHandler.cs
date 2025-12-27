using Microsoft.EntityFrameworkCore;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Accounts.GetBalance;

public sealed class GetBalanceHandler(PaymentsDbContext db) : IGetBalanceHandler
{
    public async Task<GetBalanceResponse?> Handle(string userId, CancellationToken ct)
    {
        var data = await db.Accounts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new GetBalanceResponse(x.UserId, x.Balance))
            .FirstOrDefaultAsync(ct);
        return data;
    }
}