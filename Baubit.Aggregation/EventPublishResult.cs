namespace Baubit.Aggregation
{
    public record EventPublishResult(bool Success, string Supplement = "");
    internal static class EventPublishResults
    {
        internal static EventPublishResult AggregatorDisposed => new EventPublishResult(false, "Aggregator disposed");
        internal static EventPublishResult AggregatorCapacityReached => new EventPublishResult(false, "Max capacity reached");
        internal static EventPublishResult UnexpectedWriteFailure => new EventPublishResult(false, "Unexpected write failure");
        internal static EventPublishResult CancelledByCaller => new EventPublishResult(false, "Cancelled by caller");
        internal static EventPublishResult WriteTimedOut => new EventPublishResult(false, "Write timed out");
        internal static EventPublishResult Successful => new EventPublishResult(true);
    }
}
