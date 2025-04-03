using Baubit.Traceability.Reasons;

namespace Baubit.Aggregation.ResultReasons
{
    public sealed class WriteTimedOut : AReason
    {
        public WriteTimedOut() : base("Write timed out", default)
        {
        }
    }
}
