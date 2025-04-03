using Baubit.Traceability.Reasons;

namespace Baubit.Aggregation.ResultReasons
{
    public sealed class CancelledByCaller : AReason
    {
        public CancelledByCaller() : base("Cancelled by caller", default)
        {
        }
    }
}
