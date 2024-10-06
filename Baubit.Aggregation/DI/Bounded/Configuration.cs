using System.Threading.Channels;

namespace Baubit.Aggregation.DI.Bounded
{
    public class Configuration : AConfiguration
    {
        public int Capacity { get; set; }
        public BoundedChannelFullMode FullMode { get; set; }
        public int MaxWaitToWriteMS { get; set; }
    }
}
