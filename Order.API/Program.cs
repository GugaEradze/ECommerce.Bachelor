using MassTransit;
using Marten;
using ECommerce.Shared;
using Microsoft.AspNetCore.Mvc;
using Order.API.StateMachines; 
using Order.API.Dtos;          

Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:5001");

var builder = WebApplication.CreateBuilder(args);

var dbConnectionString = builder.Configuration.GetConnectionString("OrderDatabase")
    ?? throw new InvalidOperationException("Postgres პაროლები ვერ მოიძებნა!");

var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

builder.Services.AddMarten(options =>
{
    options.Connection(dbConnectionString);
});

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderSagaState>()
        .MartenRepository();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapPost("/api/orders", async (IDocumentSession session, IPublishEndpoint publishEndpoint, [FromBody] CreateOrderDto dto) =>
{
    var orderId = Guid.NewGuid();

    var orderCreatedEvent = new OrderCreatedEvent
    {
        OrderId = orderId,
        ProductId = dto.ProductId,
        Quantity = dto.Quantity,
        Price = dto.Price
    };

    session.Events.StartStream<OrderSagaState>(orderId, orderCreatedEvent);
    await session.SaveChangesAsync();

    await publishEndpoint.Publish(orderCreatedEvent);

    return Results.Accepted($"/api/orders/{orderId}", new { orderId, status = "შეკვეთის დამუშავება დაიწყო..." });
});

app.Run();