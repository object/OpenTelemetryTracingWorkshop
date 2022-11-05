using System;
using System.Diagnostics;
using System.Text;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace QueueConsumer
{
    internal class MessageHandler : BackgroundService
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource(nameof(MessageHandler));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly IBus _bus;

        public MessageHandler(IBus bus) => _bus = bus;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queue = GetQueue();

            using var subscription = _bus.Advanced.Consume(
                queue,
                async (messageBytes, properties, receivedInfo) =>
                {
                    Console.WriteLine($"Received {messageBytes.Length} bytes");

                    // Extract the PropagationContext of the upstream parent from the message headers
                    var parentContext = Propagator.Extract(default, properties, ExtractTraceContext);

                    // Inject extracted info into current context
                    Baggage.Current = parentContext.Baggage;

                    // start an activity
                    using var activity = ActivitySource.StartActivity("mq_receive", ActivityKind.Consumer, parentContext.ActivityContext, tags: new[] { new KeyValuePair<string, object?>("server", Environment.MachineName) });

                    AddMessagingTags(activity, receivedInfo);
                });

            await UntilCancelled(stoppingToken);

            Queue GetQueue()
            {
                var queue = _bus.Advanced.QueueDeclare("words");
                var exchange = _bus.Advanced.ExchangeDeclare("words", ExchangeType.Topic);
                var binding = _bus.Advanced.Bind(exchange, queue, "*");
                return queue;
            }

            IEnumerable<string> ExtractTraceContext(MessageProperties properties, string key)
            {
                try
                {
                    if (properties.Headers.TryGetValue(key, out var value) && value is byte[] bytes)
                    {
                        return new[] { Encoding.UTF8.GetString(bytes) };
                    }
                }
                catch (Exception ex)
                {
                }

                return Enumerable.Empty<string>();
            }

            static void AddMessagingTags(Activity? activity, MessageReceivedInfo receivedInfo)
            {
                // https://github.com/open-telemetry/opentelemetry-dotnet/tree/core-1.1.0/examples/MicroserviceExample/Utils/Messaging
                // Following OpenTelemetry messaging specification conventions
                // See:
                //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes
                //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#rabbitmq

                activity?.SetTag("messaging.system", "rabbitmq");
                activity?.SetTag("messaging.destination_kind", "queue");
                activity?.SetTag("messaging.destination", receivedInfo.Exchange);
                activity?.SetTag("messaging.rabbitmq.routing_key", receivedInfo.RoutingKey);
            }

            static async Task UntilCancelled(CancellationToken ct)
            {
                var tcs = new TaskCompletionSource<bool>();
                using var ctRegistration = ct.Register(() => tcs.SetResult(true));
                await tcs.Task;
            }
        }
    }
}
