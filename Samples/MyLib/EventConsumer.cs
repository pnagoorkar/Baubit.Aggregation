namespace MyLib
{
    public class EventConsumer : Baubit.Aggregation.IObserver<Event>
    {
        private static int consumerIDSeed = 0;
        public int ConsumerID { get; set; }
        private IDisposable subscription;
        public EventConsumer(Baubit.Aggregation.IObservable<Event> observable)
        {
            ConsumerID = ++consumerIDSeed;
            subscription = observable.TrySubscribeAsync(this).GetAwaiter().GetResult().Value;
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
            Console.WriteLine($"Consumer {ConsumerID} received event: {value.EventId}");
        }
    }
}
