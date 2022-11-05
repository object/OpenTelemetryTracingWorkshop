using System.Diagnostics;
using EasyNetQ;
using Honeycomb.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using QueueConsumer;

// Define some important constants and the activity source
var serviceName = "OTelTest_QueueConsumer";
var serviceVersion = "1.0.0";


var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IBus>(_ => RabbitHutch.CreateBus("host=localhost"));
        services.AddOpenTelemetryTracing(builder =>
        {
            builder
                .AddSource(nameof(MessageHandler))
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                .AddJaegerExporter(o =>
                {
                    o.AgentHost = "localhost";
                    o.AgentPort = 6831;
                })
                .AddHoneycomb(new HoneycombOptions
                {
                    ServiceName = serviceName,
                    ApiKey = "<INSERT_API_KEY>",
                    Dataset = "my-dataset"
                })
                .AddConsoleExporter();
        });
        // OpenTelemetry adds an hosted service of its own, so we should register it before our hosted services,
        // or we might start handing messages (or whatever our background service does) before tracing is up and running
        // (noticed this as when I stopped this service and sent messages to the queue, the first message handled
        // wouldn't show up in the traces)
        services.AddHostedService<MessageHandler>();
    });

var host = builder.Build();

await host.RunAsync();
