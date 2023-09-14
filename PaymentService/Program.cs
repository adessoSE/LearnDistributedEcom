using System.Reflection;

using MassTransit;

using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

using PaymentService.Sagas.Payment;

using Serilog;

using SharedConfiguration;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    Console.Title = "Payment Service";
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

builder.Services.AddSingleton(provider =>
{
    var clientSettings = MongoClientSettings.FromUrl(new MongoUrl("mongodb://root:Pass!!word@127.0.0.1"));
    var options = new InstrumentationOptions() { CaptureCommandText = true };

    clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber(options));
    var mongoClient = new MongoClient(clientSettings);

    return mongoClient;
});

builder.Services.AddMassTransit(config =>
{
    //config.UsingRabbitMq((context, cfg) =>
    //{
    //    cfg.ConfigureEndpoints(context);
    //});

    config.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(builder.Configuration["AzureServiceBusConnectionString"]);
        cfg.ConfigureEndpoints(context);
    });

    config.AddConsumers(Assembly.GetExecutingAssembly());

    config.AddSagaStateMachine<PaymentStateMachine, PaymentState>()
        .MongoDbRepository(config =>
           {
               config.Connection = "mongodb://root:Pass!!word@127.0.0.1";
               config.DatabaseName = "paymentdb";
               config.CollectionName = "payments";
               config.ClientFactory(provider => provider.GetService<MongoClient>());
           });
});

builder.Services.AddOpenTelemetry("Payment", builder.Configuration);

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

app.Run();
