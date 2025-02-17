namespace Baubit.Aggregation.DI
{
    public abstract class AConfiguration : Baubit.DI.AConfiguration
    {
        public AggregatorConfiguration AggregatorConfiguration { get; init; }
    }
}
