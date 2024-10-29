using Baubit.Traceability;

namespace Baubit.Aggregation.Traceability
{
    public class Received : ATraceEvent
    {

    }

    public class DispatchCancelled : ATraceEvent
    {

    }

    public class OutForDispatch : ATraceEvent
    {

    }

    public class Dispatched : ATraceEvent
    {

    }

    public class OutForDelivery : ATraceEvent
    {
        public object Target { get; init; }
        public OutForDelivery(object target)
        {
            Target = target;
        }
    }

    public class Delivered : ATraceEvent
    {
        public object Target { get; init; }
        public Delivered(object target)
        {
            Target = target;
        }
    }
}
