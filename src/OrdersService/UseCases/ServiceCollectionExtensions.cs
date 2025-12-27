using OrdersService.UseCases.ApplyPaymentResult;
using OrdersService.UseCases.CreateOrder;
using OrdersService.UseCases.GetOrderStatus;
using OrdersService.UseCases.ListOrders;

namespace OrdersService.UseCases;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<ICreateOrderHandler, CreateOrderHandler>();
        services.AddScoped<IListOrdersHandler, ListOrdersHandler>();
        services.AddScoped<IGetOrderStatusHandler, GetOrderStatusHandler>();
        services.AddScoped<ApplyPaymentResultHandler>();

        return services;
    } 
}