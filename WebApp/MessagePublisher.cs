using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace MqPublisher
{
    internal class MessagePublisher : IDisposable
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource(nameof(MessagePublisher));
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        private readonly IBus _bus;
        private readonly Lazy<Exchange> _exchange;

        public MessagePublisher()
        {
            _bus = RabbitHutch.CreateBus("host=localhost");
            _exchange = new Lazy<Exchange>(() => _bus.Advanced.ExchangeDeclare("words", ExchangeType.Topic));
        }

        public void Dispose() => _bus.Dispose();

        public async Task PublishAsync<T>(T message)
        {
            using var activity = ActivitySource.StartActivity("mq_publish", ActivityKind.Producer);
            var messageProperties = new MessageProperties();

            ActivityContext contextToInject = activity?.Context ?? Activity.Current?.Context ?? default;

            // Inject the ActivityContext into the message headers to propagate trace context to the receiving service.
            Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), messageProperties, InjectTraceContext);

            await _bus.Advanced.PublishAsync(_exchange.Value, "words", false, new Message<T>(message, messageProperties));

            void InjectTraceContext(MessageProperties messageProperties, string key, string value)
            {
                if (messageProperties.Headers is null)
                {
                    messageProperties.Headers = new Dictionary<string, object>();
                }

                messageProperties.Headers[key] = value;
            }
        }
    }
}
