using Microsoft.Extensions.Logging;

namespace MyLib
{
    public class EventConsumer : Baubit.Aggregation.IObserver<Event>
    {
        private static int consumerIDSeed = 0;
        public int ConsumerID { get; set; }
        private IDisposable subscription;
        private readonly ILogger<EventConsumer> _logger;
        public EventConsumer(Baubit.Aggregation.IObservable<Event> observable, ILogger<EventConsumer> logger)
        {
            ConsumerID = ++consumerIDSeed;
            subscription = observable.TrySubscribeAsync(this).GetAwaiter().GetResult().Value;
            _logger = logger;
        }
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public async Task OnNext(Event value, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Consumer {ConsumerID} received event: {value.EventId}");
        }
    }
}
