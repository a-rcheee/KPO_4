using Microsoft.EntityFrameworkCore;
using PaymentsService.Entities;

namespace PaymentsService.Infrastructure.Data;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<InboxMessage> Inbox => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("accounts");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.UserId).IsUnique();
            b.Property(x => x.Balance).HasColumnName("balance");
            b.Property(x => x.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.PublishedAtUtc);
        });
        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.ToTable("inbox");
            b.HasKey(x => x.MessageId);
        });
    }
}