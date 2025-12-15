namespace Watcher.Runner.Tests.Unit;

[TestFixture]
public class EventBusTests
{
    [Test]
    public async Task PublishShouldInvokeSubscription()
    {
        var bus = new EventBus();
        var handlerInvoked = false;

        bus.Subscribe<TestEvent>(x => handlerInvoked = true);
        bus.Publish(new TestEvent());
        await Task.Delay(50);

        Assert.That(handlerInvoked, Is.True, "Handler is invoked");
    }
}

public record TestEvent : IEvent;