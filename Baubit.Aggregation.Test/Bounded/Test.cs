using Baubit.Aggregation.Test.AEventAggregator;
using Baubit.xUnit;
using Xunit.Abstractions;

namespace Baubit.Aggregation.Test.Bounded
{
    public class Test : ATest<Broker>
    {
        public Test(Fixture<Broker> fixture, ITestOutputHelper testOutputHelper, IMessageSink diagnosticMessageSink = null) : base(fixture, testOutputHelper, diagnosticMessageSink)
        {
        }
    }
}
