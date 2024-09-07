using System.Text;

namespace Baubit.Aggregation
{
    public static class EventAggregatorExtensions
    {
        //public static void Validate(this EventAggregatorOptions options)
        //{
        //    if (options != null)
        //    {
        //        if (options?.AggregationBufferSize != null && options.AggregationMaxWaitTime == null)
        //        {
        //            var stringBuilder = new StringBuilder();
        //            stringBuilder.AppendLine($"{nameof(EventAggregatorOptions.AggregationMaxWaitTime)} cannot be null when {nameof(EventAggregatorOptions.AggregationBufferSize)} is defined.");
        //            stringBuilder.AppendLine($"For a bounded Aggregator, both, {nameof(EventAggregatorOptions.AggregationBufferSize)} and {nameof(EventAggregatorOptions.AggregationMaxWaitTime)} are required.");
        //            stringBuilder.AppendLine($"For an unbounded Aggregator, both, {nameof(EventAggregatorOptions.AggregationBufferSize)} and {nameof(EventAggregatorOptions.AggregationMaxWaitTime)} must be null.");

        //            throw new ArgumentException(stringBuilder.ToString());
        //        }
        //        if (options?.AggregationBufferSize == null && options.AggregationMaxWaitTime != null)
        //        {
        //            var stringBuilder = new StringBuilder();
        //            stringBuilder.AppendLine($"{nameof(EventAggregatorOptions.AggregationBufferSize)} cannot be null when {nameof(EventAggregatorOptions.AggregationMaxWaitTime)} is defined");
        //            stringBuilder.AppendLine($"For a bounded Aggregator, both, {nameof(EventAggregatorOptions.AggregationBufferSize)} and {nameof(EventAggregatorOptions.AggregationMaxWaitTime)} are required.");
        //            stringBuilder.AppendLine($"For an unbounded Aggregator, both, {nameof(EventAggregatorOptions.AggregationBufferSize)} and {nameof(EventAggregatorOptions.AggregationMaxWaitTime)} must be null.");

        //            throw new ArgumentException(stringBuilder.ToString());
        //        }
        //    }
        //}

        public static void Dispose<TEvent>(this IList<AEventDispatcher<TEvent>> dispatchers)
        {
            for (int i = 0; i < dispatchers.Count; i++)
            {
                dispatchers[i].Dispose();
            }
        }
    }
}
