using System.Text;
using System.Text.Json;
using Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.Entities;
using OrdersService.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrdersService.Messaging;

public sealed class PaymentResultsConsumerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<PaymentResultsConsumerBackgroundService> _log;
    private readonly TimeProvider _time;

    public PaymentResultsConsumerBackgroundService(IServiceProvider sp, IOptions<RabbitMqOptions> options, ILogger<PaymentResultsConsumerBackgroundService> log, TimeProvider time)
    {
        _sp = sp;
        _options = options.Value;
        _log = log;
        _time = time;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            UserName = _options.User,
            Password = _options.Password
        };
        while (!stoppingToken.IsCancellationRequested)
        {
            IConnection? connection = null;
            IChannel? channel = null;
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                await channel.QueueDeclareAsync(queue: Queues.PaymentResults, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.Span);
                        var envelope = JsonSerializer.Deserialize<MessageEnvelope>(json);
                        if (envelope is null)
                        {
                            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
                            return;
                        }
                        var now = _time.GetUtcNow().UtcDateTime;
                        var result = envelope.ReadPayload<PaymentResult>();

                        using var scope = _sp.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
                        await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);
                        var already = await db.Inbox.AnyAsync(x => x.MessageId == envelope.MessageId, stoppingToken);
                        if (!already)
                        {
                            var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == result.OrderId, stoppingToken);
                            if (order is not null)
                            {
                                if (order.Status == OrderStatus.PendingPayment)
                                {
                                    order.Status = result.Success ? OrderStatus.Paid : OrderStatus.Failed;
                                    order.UpdatedAtUtc = now;
                                }
                            }
                            else
                            {
                                _log.LogWarning("Order not found for payment result. order={OrderId}", result.OrderId);
                            }

                            db.Inbox.Add(new InboxMessage
                            {
                                MessageId = envelope.MessageId,
                                ReceivedAtUtc = now
                            });

                            await db.SaveChangesAsync(stoppingToken);
                            await tx.CommitAsync(stoppingToken);
                        }
                        else
                        {
                            await tx.CommitAsync(stoppingToken);
                        }

                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Failed to process PaymentResult");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: Queues.PaymentResults,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "RabbitMQ not ready");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            finally
            {
                try
                {
                    if (channel is not null)
                        await channel.CloseAsync(stoppingToken);
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if (connection is not null)
                    {
                        await connection.CloseAsync(
                            reasonCode: 200,
                            reasonText: "Closing",
                            timeout: TimeSpan.FromSeconds(5),
                            abort: false,
                            cancellationToken: stoppingToken);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}