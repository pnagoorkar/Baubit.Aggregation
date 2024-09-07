namespace Baubit.Aggregation
{
    public static class EventAggregatorExtensions
    {
        public static void Dispose<TEvent>(this IList<AEventDispatcher<TEvent>> dispatchers)
        {
            for (int i = 0; i < dispatchers.Count; i++)
            {
                dispatchers[i].Dispose();
            }
        }
    }
}
