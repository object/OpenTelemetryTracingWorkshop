using System.Diagnostics;
using Honeycomb.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

// Define some important constants and the activity source
//var serviceName = "OTelTest_Console";
var serviceName = "oteltest-console";
var serviceVersion = "1.0.0";


async Task startActivity(ActivitySource activitySource, IReadOnlyList<string> words, int count)
{
    if (count == words.Count) return;
    Console.WriteLine(words[count]);
    using var activity = activitySource.StartActivity($"word_activity_{count}");
    activity?.SetTag("count", count);
    activity?.SetTag("word", words[count]);
    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(100)));
    await startActivity(activitySource, words, count + 1);
}

// Configure OpenTelemetry settings
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault() 
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddJaegerExporter(o =>
    {
        o.AgentHost = "localhost";
        o.AgentPort = 6831;
    })
    .AddConsoleExporter()

    .AddHoneycomb(new HoneycombOptions
    {
        ServiceName = serviceName,
        ApiKey = "<INSERT_API_KEY>",
        Dataset = "my-dataset"
    })
    
    .Build();

await Task.Delay(1000);

var words = new List<string>() { "quick", "brown", "fox", "jumped", "over", "the", "lazy", "dog"};

var MyActivitySource = new ActivitySource(serviceName);

for (var i = 0; i < 10; i++)
    await startActivity(MyActivitySource, words, 0);
