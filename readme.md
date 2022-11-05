# OpenTelemetry tests
## Intro
Simple set of ASP.NET Core applications to explore distributed tracing support, as well as the standard [W3C Trace Context](https://www.w3.org/TR/trace-context/) usage.

1. `ConsoleApp` - stand-alone app with a single service.

## Viewing the trace context

### Logs

The simplest way to see this in action is looking at the logs, as we can then see this information printed out.

Even better than seeing things printed in the console, is to centralize the logs, then use the filters to find the logs we want. To see it in action, we're using [Seq](https://datalust.co/seq).

### Jaeger

For this sample, we'll use [OpenTelemetry](https://opentelemetry.io/) and [Jaeger](https://www.jaegertracing.io/).

OpenTelemetry provides a set of tools to capture the trace information and then make available to observability tools (like Zipkin and Jaeger). OpenTelemetry provides a set of [NuGet packages](https://github.com/open-telemetry/opentelemetry-dotnet) we can use to easily integration with .NET.

Jaeger UI: http://localhost:16686

### MySQL

docker exec -it opentelemetrytests-mysql-1 bash
mysql -uuser -pPassword1

### Getting things running

Before running the .NET applications, we need to have our dependencies up, which in this case are RabbitMQ, PostgreSQL, Seq, Zipkin and Jaeger. To get them all running, there's a Docker Compose file in the repository root, so we just need to execute:

```
docker compose -f docker-compose.yml up
```

### Seeding the database
```
dotnet ef migrations add SeedData
dotnet ef database update
```

### Starting applications

```
.\WebAppClient\bin\debug\net6.0\WebAppClient.exe
.\QueueConsumer\bin\Debug\net6.0\QueueConsumer.exe
```

## Other resources

To do this exploration in general, but particularly the RabbitMQ bits, relied heavily on the docs and examples provided in the [OpenTelemetry .NET repository](https://github.com/open-telemetry/opentelemetry-dotnet), like the [OpenTelemetry Example Application](https://github.com/open-telemetry/opentelemetry-dotnet/tree/core-1.1.0/examples/MicroserviceExample).
