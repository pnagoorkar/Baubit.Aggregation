namespace Baubit.Aggregation
{
    public record EventDispatchResult(bool Success, string Supplement = "");
    internal static class EventDispatchResults
    {
        internal static EventDispatchResult Successful = new EventDispatchResult(true, "Successful");
        internal static EventDispatchResult CancelledByCaller = new EventDispatchResult(false, "Cancelled by caller");
        internal static EventDispatchResult DispatcherDisposed = new EventDispatchResult(false, "Dispatcher disposed");
        internal static EventDispatchResult DispatcherCapacityReached => new EventDispatchResult(false, "Max capacity reached");
        internal static EventDispatchResult UnexpectedWriteFailure => new EventDispatchResult(false, "Unexpected write failure");
    }
}
