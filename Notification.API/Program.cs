using MassTransit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Text.Json;
using Notification.API.Consumers;
using Notification.API.Models;

Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:5004");

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var mongoConnStr = builder.Configuration.GetConnectionString("MongoDb")
    ?? throw new InvalidOperationException("MongoDB კონექშენ სტრინგი არ მოიძებნა!");

var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnStr));
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("NotificationDB");
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<InventoryReservedConsumer>();
    x.AddConsumer<PaymentSuccessConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("notification-logs-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
            e.ConfigureConsumer<InventoryReservedConsumer>(context);
            e.ConfigureConsumer<PaymentSuccessConsumer>(context);
            e.ConfigureConsumer<PaymentFailedConsumer>(context);
        });
    });
});

var app = builder.Build();

app.MapGet("/api/notifications/{orderId}", async (string orderId, IMongoDatabase database) =>
{
    var collection = database.GetCollection<NotificationLog>("Logs");

    if (Guid.TryParse(orderId, out Guid guid))
    {
        var logs = await collection.Find(x => x.OrderId == guid).ToListAsync();
        return Results.Ok(logs);
    }

    return Results.BadRequest("არასწორი Order ID ფორმატი");
});

app.Run();