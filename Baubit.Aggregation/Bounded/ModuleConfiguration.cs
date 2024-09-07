using System.Threading.Channels;

namespace Baubit.Aggregation.Bounded
{
    public class ModuleConfiguration : AModuleConfiguration
    {
        public int Capacity { get; set; }
        public BoundedChannelFullMode FullMode { get; set; }
        public int MaxWaitToWriteMS { get; set; }
    }
}
