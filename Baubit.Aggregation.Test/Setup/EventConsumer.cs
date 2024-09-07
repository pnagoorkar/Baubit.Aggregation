namespace Baubit.Aggregation.Test.Setup
{
    public class EventConsumer : IObserver<TestEvent>, IDisposable
    {
        public int Id { get; private init; }

        private static int idSeed = 1;
        private IDisposable? subscription;

        public Task Completion { get; private init; }

        private TaskCompletionSource _taskCompletionSource;
        public EventConsumer(IObservable<TestEvent> observable)
        {
            Id = idSeed++;
            subscription = observable?.Subscribe(this);
            _taskCompletionSource = new TaskCompletionSource();
            Completion = _taskCompletionSource.Task;
        }

        public void OnCompleted()
        {
            _taskCompletionSource.SetResult();
        }

        public void OnError(Exception error)
        {

        }

        public virtual void OnNext(TestEvent value)
        {
            value.Trace.Add(new Receipt(Id, DateTime.Now));
        }

        public virtual void Dispose()
        {
            subscription?.Dispose();
            subscription = null;
        }
    }
}
