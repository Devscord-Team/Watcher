using Watcher.Runner.DiscordEventHandlers;

namespace Watcher.Runner.Tests.Unit;

[TestFixture]
public class EventBusTests
{
    [Test]
    public void Test1()
    {
        var bus = new EventBus();
        bus.Subscribe<MessageInfoReceivedEvent>(x => { });
    }
}
