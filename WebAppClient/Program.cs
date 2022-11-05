using System.Diagnostics;
using Honeycomb.OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;

// Define some important constants and the activity source
var serviceName = "OTelTest_WebClient";
var serviceVersion = "1.0.0";
var wordCount = 8;

var useRootActivity = false;

async Task startActivity(ActivitySource activitySource, HttpClient httpClient, int count)
{
    using var activity = activitySource.StartActivity($"word_activity_{count}");
    var word = await httpClient.GetStringAsync("word");
    Console.WriteLine(word);
    activity?.SetTag("count", count);
    activity?.SetTag("word", word);
}

// Configure important OpenTelemetry settings and the console exporter
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddHttpClientInstrumentation()
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
    .AddConsoleExporter()
    .Build();

var MyActivitySource = new ActivitySource(serviceName);
Activity rootActivity = null;

var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("http://localhost:5188");
await httpClient.GetStringAsync("reset");


if (useRootActivity)
{
    rootActivity = MyActivitySource.StartActivity($"sentence");
    rootActivity?.SetTag("sentence", "quick brown fox jumps over the lazy dog");
}

for (var i = 0; i < wordCount; i++)
    await startActivity(MyActivitySource, httpClient, i);


if (useRootActivity)
    rootActivity.Dispose();
