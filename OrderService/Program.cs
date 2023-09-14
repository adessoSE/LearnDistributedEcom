using Microsoft.EntityFrameworkCore;
using OrderService.Persistence;

using Serilog;
using MassTransit;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SharedConfiguration;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    Console.Title = "Order Service";
}

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDbContext>(config =>
{
    config.EnableSensitiveDataLogging();
    config.UseSqlServer(builder.Configuration.GetConnectionString("SqlDb"));
});

builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

    //config.UsingAzureServiceBus((context, cfg) =>
    //{
    //    cfg.Host(builder.Configuration["AzureServiceBusConnectionString"]);
    //    cfg.ConfigureEndpoints(context);
    //});

    config.AddConsumers(Assembly.GetExecutingAssembly());
});

builder.Services.AddOpenTelemetry("Order", builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MigrateDbContext<OrderDbContext>();

app.Run();
