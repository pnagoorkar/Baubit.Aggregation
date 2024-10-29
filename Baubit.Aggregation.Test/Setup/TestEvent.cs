using Baubit.Collections;
using Baubit.Traceability;
namespace Baubit.Aggregation.Test.Setup
{
    public class TestEvent : ITraceable
    {
        public int Id { get; private init; }
        public DateTime CreatedAt { get; private init; } = DateTime.Now;
        public DateTime PostedAt { get; set; }
        public ICollection<Receipt> Trace { get; init; } = new ConcurrentList<Receipt>();

        public ObservableConcurrentStack<ITraceEvent> History { get; } = new ObservableConcurrentStack<ITraceEvent>();

        public bool EnableTrace { get; set; }

        private static int idSeed = 1;

        public TestEvent()
        {
            Id = idSeed++;
        }
    }
}
