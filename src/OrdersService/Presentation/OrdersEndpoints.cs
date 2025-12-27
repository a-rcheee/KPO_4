using Microsoft.AspNetCore.Mvc;
using OrdersService.UseCases.CreateOrder;
using OrdersService.UseCases.GetOrderStatus;
using OrdersService.UseCases.ListOrders;

namespace OrdersService.Presentation;

public static class OrdersEndpoints
{
    public static WebApplication MapOrdersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders");

        group.MapPost("",
            async ([FromHeader(Name = "User-Id")] string? userId,
                CreateOrderRequest request,
                ICreateOrderHandler handler,
                CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Results.BadRequest("Missing User-Id header");
                }

                var response = await handler.Handle(userId, request, ct);
                return Results.Ok(response);
            })
            .WithName("CreateOrder")
            .WithSummary("Create order")
            .WithOpenApi();

        group.MapGet("",
            async ([FromHeader(Name = "User-Id")] string? userId,
                IListOrdersHandler handler,
                CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Results.BadRequest("Missing User-Id header");
                }

                var response = await handler.Handle(userId, ct);
                return Results.Ok(response);
            })
            .WithName("ListOrders")
            .WithSummary("List orders")
            .WithOpenApi();

        group.MapGet("{orderId:guid}",
            async ([FromHeader(Name = "User-Id")] string? userId,
                Guid orderId,
                IGetOrderStatusHandler handler,
                CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Results.BadRequest("Missing User-Id header");
                }

                var response = await handler.Handle(userId, orderId, ct);
                return Results.Ok(response);
            })
            .WithName("GetOrderStatus")
            .WithSummary("Get order status")
            .WithOpenApi();

        return app;
    }
}