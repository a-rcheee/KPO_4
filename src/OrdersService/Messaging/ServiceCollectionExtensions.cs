using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrdersService.Infrastructure;
using RabbitMQ.Client;

namespace OrdersService.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<OrdersDbContext>(opt => opt.UseNpgsql(config.GetConnectionString("OrdersDb")));
        services.Configure<RabbitMqOptions>(config.GetSection("RabbitMq"));
        services.AddSingleton<IConnection>(sp =>
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
        services.AddHostedService<OutboxPublisherBackgroundService>();
        services.AddHostedService<PaymentResultsConsumerBackgroundService>();
        return services;
    }
}