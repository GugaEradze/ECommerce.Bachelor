using MassTransit;
using Payment.API.Consumers;
using Payment.API.Validators;

Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:5003");

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

builder.Services.AddSingleton<IPaymentValidator, DefaultPaymentValidator>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessPaymentConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("payment-queue", e =>
        {
            e.ConfigureConsumer<ProcessPaymentConsumer>(context);
        });
    });
});

var app = builder.Build();

app.Run();