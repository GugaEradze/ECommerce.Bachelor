using MassTransit;
using Inventory.API.Consumers;
using Inventory.API.Services;
using Inventory.API.Protos;
using Grpc.Core;

Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:5002");

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

builder.Services.AddGrpc();

builder.Services.AddSingleton<IStockChecker, DefaultStockChecker>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ReserveInventoryConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("inventory-reservation-queue", e =>
        {
            e.ConfigureConsumer<ReserveInventoryConsumer>(context);
        });
    });
});

var app = builder.Build();

app.MapGet("/api/inventory/check", (string productId, int quantity, IStockChecker stockChecker) =>
{
    bool isAvailable = stockChecker.IsProductAvailable(productId, quantity);
    return Results.Ok(new { isAvailable });
});

app.MapGrpcService<InventoryGrpcService>();

app.Run();

public class InventoryGrpcService : InventoryGrpc.InventoryGrpcBase
{
    private readonly IStockChecker _stockChecker;

    public InventoryGrpcService(IStockChecker stockChecker)
    {
        _stockChecker = stockChecker;
    }

    public override Task<StockResponse> CheckStock(StockRequest request, ServerCallContext context)
    {
        bool isAvailable = _stockChecker.IsProductAvailable(request.ProductId, request.Quantity);
        return Task.FromResult(new StockResponse { IsAvailable = isAvailable });
    }
}