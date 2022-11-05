using System.Diagnostics;
using Honeycomb.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using MqPublisher;
using MySqlConnector;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebApp;

// Define some important constants and the activity source
var serviceName = "OTelTest_WebApp";
var serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WordDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// Configure OpenTelemetry settings
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(serviceName)
    .AddSource(nameof(MessagePublisher))
    .AddSource("MySqlConnector")
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddAspNetCoreInstrumentation()
    .AddSqlClientInstrumentation(s => s.SetDbStatementForText = true)
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

var app = builder.Build();

var words = new[] {"quick", "brown", "fox", "jumped", "over", "the", "lazy", "dog"};

var wordIndex = 0;

var MyActivitySource = new ActivitySource(serviceName);

app.MapGet("/word", async (WordDbContext dbContext) =>
    {
        using var activity = MyActivitySource.StartActivity($"get_word");
        string result = null;
        switch (WordDbContext.Backend)
        {
            case Backend.None:
                result = words[wordIndex++ % words.Length];
                break;
            case Backend.SqlServer:
                var current = dbContext.Words.Find(wordIndex++ % words.Length + 1);
                result = current.Word;
                break;
            case Backend.MySql:
                using (var connection = new MySqlConnection("server=127.0.0.1;uid=user;pwd=Password1;database=words"))
                {
                    connection.Open();
                    using (var command = new MySqlCommand($"SELECT Word FROM Words WHERE Id={wordIndex++ % words.Length + 1};", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        result = reader.GetString(0);
                    }
                }
                break;
        }
        activity?.SetTag("word", result);
        using (var publisher = new MessagePublisher())
        {
            await publisher.PublishAsync(result);
        }
        return result;
    });

app.MapGet("/reset", () =>
    {

        wordIndex = 0;
        return "ok";

    });

app.Run();
