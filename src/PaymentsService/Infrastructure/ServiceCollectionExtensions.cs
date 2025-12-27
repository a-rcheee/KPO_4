using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentsService.Infrastructure.Data;
using PaymentsService.Infrastructure.Messaging;
using RabbitMQ.Client;

namespace PaymentsService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<PaymentsDbContext>(opt => opt.UseNpgsql(config.GetConnectionString("PaymentsDb")));
        services.Configure<RabbitMqOptions>(config.GetSection("RabbitMq"));
        services.AddSingleton<RabbitMQ.Client.IConnection>(sp =>
        {
            var o = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            var factory = new ConnectionFactory
            {
                HostName = o.Host,
                UserName = o.User,
                Password = o.Password
            };

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddHostedService<PaymentRequestsConsumerBackgroundService>();
        services.AddHostedService<OutboxPublisherBackgroundService>();

        return services;
    }
}