namespace Baubit.Aggregation.Test.Setup
{

    public class BusyConsumer : EventConsumer
    {
        CancellationTokenSource _masterCancellationTokenSource = new CancellationTokenSource();
        public BusyConsumer(IObservable<TestEvent> observable) : base(observable)
        {
        }
        public override void OnNext(TestEvent value)
        {
            while (!_masterCancellationTokenSource.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }
        }
        public override void Dispose()
        {
            _masterCancellationTokenSource.Cancel();
            base.Dispose();
        }
    }
}
