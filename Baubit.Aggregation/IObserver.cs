namespace Baubit.Aggregation
{
    public interface IObserver<in T>
    {
        void OnCompleted();

        void OnError(Exception error);

        public Task OnNext(T value, CancellationToken cancellationToken);
    }
}
