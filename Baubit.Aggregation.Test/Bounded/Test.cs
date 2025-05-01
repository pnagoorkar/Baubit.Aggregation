using Baubit.Aggregation.Test.AEventAggregator;
using Baubit.xUnit;
using Xunit.Abstractions;

namespace Baubit.Aggregation.Test.Bounded
{
    public class Test : ATest<Context>
    {
        public Test(Fixture<Context> fixture, ITestOutputHelper testOutputHelper, IMessageSink diagnosticMessageSink = null) : base(fixture, testOutputHelper, diagnosticMessageSink)
        {
        }
    }
}

