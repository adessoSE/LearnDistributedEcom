namespace SharedConfiguration;

using System;
using System.Diagnostics;
using MassTransit.Metadata;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using SharedConfiguration;

using OpenTelemetry.Trace;

public static class TelemetryConfigurationExtensions
{
    /// <summary>
    /// Configure Open Telemetry on the service
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName">name for this service</param>
    public static void AddOpenTelemetry(this IServiceCollection services, string serviceName, IConfiguration configuration)
    {
        var applicationInsightsConnectionString = configuration["ApplicationInsightsConnectionString"];

        services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder
                    .AddMeter("MassTransit")
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName)
                        .AddTelemetrySdk()
                        .AddEnvironmentVariableDetector())
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(HostMetadataCache.IsRunningInContainer ? "http://grafana-agent:14317" : "http://localhost:12345");
                        o.Protocol = OtlpExportProtocol.Grpc;
                        o.ExportProcessorType = ExportProcessorType.Batch;
                        o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                        {
                            MaxQueueSize = 2048,
                            ScheduledDelayMilliseconds = 5000,
                            ExporterTimeoutMilliseconds = 30000,
                            MaxExportBatchSize = 512,
                        };
                    })
                    .AddAzureMonitorMetricExporter(o =>
                    {
                        o.ConnectionString = applicationInsightsConnectionString;
                    });
            })
            .WithTracing(builder =>
            {
                builder
                    .AddSource("MassTransit")
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName)
                        .AddTelemetrySdk()
                        .AddEnvironmentVariableDetector())
                    .AddAspNetCoreInstrumentation()
                    .AddSqlClientInstrumentation(o =>
                    {
                        o.EnableConnectionLevelAttributes = true;
                        o.RecordException = true;
                        o.SetDbStatementForText = true;
                        o.SetDbStatementForStoredProcedure = true;                        
                    })
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(HostMetadataCache.IsRunningInContainer ? "http://tempo:4317" : "http://localhost:4317");
                        o.Protocol = OtlpExportProtocol.Grpc;
                        o.ExportProcessorType = ExportProcessorType.Batch;
                        o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                        {
                            MaxQueueSize = 2048,
                            ScheduledDelayMilliseconds = 5000,
                            ExporterTimeoutMilliseconds = 30000,
                            MaxExportBatchSize = 512,
                        };
                    })
                    .AddJaegerExporter(o =>
                    {
                        o.Endpoint = new Uri(HostMetadataCache.IsRunningInContainer ? "http://jaeger:6831" : "http://localhost:6831");
                        o.ExportProcessorType = ExportProcessorType.Batch;
                        o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                        {
                            MaxQueueSize = 2048,
                            ScheduledDelayMilliseconds = 5000,
                            ExporterTimeoutMilliseconds = 30000,
                            MaxExportBatchSize = 512,
                        };
                    })
                    .AddAzureMonitorTraceExporter(o =>
                    {
                        o.ConnectionString = applicationInsightsConnectionString;
                    });
                    
                builder.AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");
            });
    }
}