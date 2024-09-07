using Baubit.xUnit;
using Xunit.Abstractions;

namespace Baubit.Aggregation.Test.AEventAggregator
{
    public abstract class ATest<TBroker> : AClassFixture<Fixture<TBroker>, TBroker> where TBroker : ABroker
    {
        protected ATest(Fixture<TBroker> fixture, ITestOutputHelper testOutputHelper, IMessageSink diagnosticMessageSink = null) : base(fixture, testOutputHelper, diagnosticMessageSink)
        {
        }
    }
}
