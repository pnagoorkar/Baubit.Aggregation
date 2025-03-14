A simple event aggregator that supports <a href="https://www.google.com/search?q=asynchronous+event+buffering" target="_blank" rel="noopener noreferrer">asynchronous event buffering</a>. 
Provides a <a href="https://github.com/pnagoorkar/Baubit" target="_blank" rel="noopener noreferrer">Baubit</a> module

# Get Started
Unbounded event aggregator<br>
myConfig.json
```json
{
    "modules": [
        {
            "type": "Baubit.Aggregation.DI.Unbounded.Module`1[[MyLib.TestEvent, MyLib]], Baubit.Aggregation",
            "configuration": {

            }
        }
    ]
}
```
Bounded event aggregator<br>
myConfig.json
```json
{
    "modules": [
        {
            "type": "Baubit.Aggregation.DI.Bounded.Module`1[[MyLib.TestEvent, MyLib]], Baubit.Aggregation",
            "configuration": {
                "Capacity": 10,
                "FullMode": 0,
                "MaxWaitToWriteMS": 10
            }
        }
    ]
}
```
MyEventGenerator.cs
```cs
public class MyEventGenerator
{
    private IEventAggregator<TestEvent> _eventAggregator;
    public MyEventGenerator(IEventAggregator<TestEvent> eventAggregator)
    {
        _eventAggregator = eventAggregator
    }

    public void DoSomething()
    {
        //_eventAggregator.TryPublishAsync(...);
    }
}
```
MyEventObserver.cs
```cs
public class MyEventObserver : IObserver<TestEvent>
{
    private IDisposable subscription;
    public EventConsumer(IObservable<TestEvent> observable)
    {
        subscription = observable?.TrySubscribeAsync(this).GetAwaiter().GetResult().Value;
    }
    public void OnCompleted()
    {

    }

    public void OnError(Exception error)
    {

    }

    public virtual void OnNext(TestEvent next)
    {
        // handle next event
    }
}
```