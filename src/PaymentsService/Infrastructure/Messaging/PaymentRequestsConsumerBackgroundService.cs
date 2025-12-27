using System.Text;
using System.Text.Json;
using Contracts;
using Microsoft.Extensions.Options;
using PaymentsService.ProcessPaymentRequest;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentsService.Infrastructure.Messaging;

public sealed class PaymentRequestsConsumerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<PaymentRequestsConsumerBackgroundService> _log;

    public PaymentRequestsConsumerBackgroundService(IServiceProvider sp, IOptions<RabbitMqOptions> options, ILogger<PaymentRequestsConsumerBackgroundService> log)
    {
        _sp = sp;
        _options = options.Value;
        _log = log;
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

                await channel.QueueDeclareAsync(
                    queue: Queues.PaymentRequests,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);
                await channel.BasicQosAsync(
                    prefetchSize: 0,
                    prefetchCount: 10,
                    global: false,
                    cancellationToken: stoppingToken);
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

                        using var scope = _sp.CreateScope();
                        var handler = scope.ServiceProvider.GetRequiredService<ProcessPaymentRequestHandler>();
                        await handler.Handle(envelope, stoppingToken);
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Failed to process PaymentRequested");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(queue: Queues.PaymentRequests, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
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
                    {
                        await channel.CloseAsync(stoppingToken);
                    }
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